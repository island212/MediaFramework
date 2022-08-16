using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MediaFramework.LowLevel.MP4
{
    public struct TimeSample
    {
        public uint count;
        public uint delta;
    }

    public struct SampleChunk
    {
        public uint firstChunk;
        public uint samplesPerChunk;
        public uint sampleDescriptionIndex;
    }

    public struct ChunkOffset
    {
        public uint value;
    }

    public struct ChunkOffset64
    {
        public ulong value;
    }

    public struct MP4VideoTrack
    {
        public uint ID;
        public uint Timescale;
        public ulong Duration;
        public ISOLanguage Language;

        public int Width, Heigth;

        public int FrameCount;

        public BlobArray<TimeSample> TimeToSampleTable;
        public BlobArray<SampleChunk> SampleToChunkTable;
        public BlobArray<ChunkOffset> ChunkOffsetTable;

        //public BlobArray<byte> SPS;
        //public BlobArray<byte> PPS;
    }

    public struct MP4AudioTrack
    {
        public uint ID;
        public uint Timescale;
        public ulong Duration;
        public ISOLanguage Language;

        public int ChannelCount;
        public int SampleRate;

        public BlobArray<TimeSample> TimeToSampleTable;
        public BlobArray<SampleChunk> SampleToChunkTable;
        public BlobArray<ChunkOffset> ChunkOffsetTable;
    }

    public struct MP4Header
    {
        public ulong Duration;
        public uint Timescale;

        public long DataOffset;

        public BlobArray<MP4VideoTrack> Videos;
        public BlobArray<MP4AudioTrack> Audios;
    }

    public enum MP4Error
    {
        None,
        FileNotFound,
        InvalidBoxType,
        InvalidBoxSize,
        InvalidBoxVersion,
        InvalidReadSize,
        InvalidEntryCount,
        OverMaxPolicy,
        DuplicateBox,
        IndexOutOfReaderRange,
        IllegalBoxDepth
    }

    public enum MP4Policy
    { 
        Soft,
        Strict
    }

    public struct MP4JobContext
    {
        public int BoxDepth;
        public int VideoTrackCount;
        public int AudioTrackCount;

        public ulong Duration;
        public uint Timescale;
        public uint NextTrackID;

        public UnsafeList<MP4JobTrackContext> Tracks;

        public JobLogger Logger;

        public ref MP4JobTrackContext CurrentTrack => ref Tracks.ElementAt(Tracks.Length - 1);

        public MP4Error LogError(MP4Error error, int index, in FixedString128Bytes message)
        {
            Logger.Log(new JobLog
            {
                Type = LogType.Error,
                MetaData1 = Tracks.Length,
                MetaData2 = (int)error,
                MetaData3 = index,
            }, message);

            return error;
        }
    }

    public struct MP4JobTrackContext
    {
        public ISOHandler Handler;
        public uint TrackID;
        public uint Timescale;
        public ulong Duration;
        public ISOLanguage Language;

        public int STSDIndex;
        public SampleArray STTS;
        public SampleArray STSC;
        public SampleArray STCO;
    }

    public struct SampleArray
    {
        public int SampleIndex;
        public int EntryCount;
    }

    public struct MP4BlobJob : IJob
    {
        public BByteReader Reader;

        public NativeReference<JobLogger> Logger;
        public NativeReference<BlobAssetReference<MP4Header>> Header;
      
        public unsafe void Execute()
        {
            var context = new MP4JobContext();
            context.Logger = Logger.Value;

            var moovBox = Reader.ReadISOBox();

            var error = ISOBMFF.Read(ref context, ref Reader, moovBox);

            var builder = new BlobBuilder(Allocator.Temp);
            ref var header = ref builder.ConstructRoot<MP4Header>();

            var videos = builder.Allocate(ref header.Videos, context.VideoTrackCount);
            var audios = builder.Allocate(ref header.Audios, context.AudioTrackCount);

            header.Duration = context.Duration;
            header.Timescale = context.Timescale;

            int videoCount = 0, audioCount = 0;
            for (int i = 0; i < context.Tracks.Length; i++)
            {
                ref var source = ref context.Tracks.ElementAt(i);

                switch (source.Handler)
                {
                    case ISOHandler.VIDE:
                        {
                            ref var dest = ref videos[videoCount++];

                            dest.ID = source.TrackID;
                            dest.Duration = source.Duration;
                            dest.Timescale = source.Timescale;
                            dest.Language = source.Language;

                            BuildTimeToSampleTable(ref Reader, ref builder, ref dest.TimeToSampleTable, source.STTS);
                            BuildSampleToChunkTable(ref Reader, ref builder, ref dest.SampleToChunkTable, source.STSC);
                            BuildChunkOffsetTable(ref Reader, ref builder, ref dest.ChunkOffsetTable, source.STCO);
                        }
                        break;
                    case ISOHandler.SOUN:
                        {
                            ref var dest = ref audios[audioCount++];

                            dest.ID = source.TrackID;
                            dest.Duration = source.Duration;
                            dest.Timescale = source.Timescale;
                            dest.Language = source.Language;

                            BuildTimeToSampleTable(ref Reader, ref builder, ref dest.TimeToSampleTable, source.STTS);
                            BuildSampleToChunkTable(ref Reader, ref builder, ref dest.SampleToChunkTable, source.STSC);
                            BuildChunkOffsetTable(ref Reader, ref builder, ref dest.ChunkOffsetTable, source.STCO);
                        }
                        break;
                }
            }

            Header.Value = builder.CreateBlobAssetReference<MP4Header>(Allocator.Persistent);
            Logger.Value = context.Logger;

            builder.Dispose();
            context.Tracks.Dispose();
        }

        public static MP4Error BuildTimeToSampleTable(ref BByteReader reader, ref BlobBuilder builder, ref BlobArray<TimeSample> sttsArray, in SampleArray stts)
        {
            reader.Index = stts.SampleIndex;
            var array = builder.Allocate(ref sttsArray, stts.EntryCount);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new TimeSample
                {
                    count = reader.ReadUInt32(),
                    delta = reader.ReadUInt32()
                };
            }

            return MP4Error.None;
        }

        public static MP4Error BuildSampleToChunkTable(ref BByteReader reader, ref BlobBuilder builder, ref BlobArray<SampleChunk> stscArray, in SampleArray stsc)
        {
            reader.Index = stsc.SampleIndex;
            var array = builder.Allocate(ref stscArray, stsc.EntryCount);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new SampleChunk
                {
                    firstChunk = reader.ReadUInt32(),
                    samplesPerChunk = reader.ReadUInt32(),
                    sampleDescriptionIndex = reader.ReadUInt32()
                };
            }

            return MP4Error.None;
        }

        public static MP4Error BuildChunkOffsetTable(ref BByteReader reader, ref BlobBuilder builder, ref BlobArray<ChunkOffset> stcoArray, in SampleArray stco)
        {
            reader.Index = stco.SampleIndex;
            var array = builder.Allocate(ref stcoArray, stco.EntryCount);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new ChunkOffset
                {
                    value = reader.ReadUInt32()
                };
            }

            return MP4Error.None;
        }
    }
}
