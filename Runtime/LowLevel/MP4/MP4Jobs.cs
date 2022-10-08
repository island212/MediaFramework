using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using System;

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

    //public struct MP4VideoTrack
    //{
    //    public uint ID;
    //    public uint Timescale;
    //    public ulong Duration;
    //    public ISOLanguage Language;

    //    public int Width, Heigth;

    //    public int FrameCount;

    //    public BlobArray<TimeSample> TimeToSampleTable;
    //    public BlobArray<SampleChunk> SampleToChunkTable;
    //    public BlobArray<ChunkOffset> ChunkOffsetTable;
    //}

    //public struct MP4AudioTrack
    //{
    //    public uint ID;
    //    public uint Timescale;
    //    public ulong Duration;
    //    public ISOLanguage Language;

    //    public int ChannelCount;
    //    public int SampleRate;

    //    public BlobArray<TimeSample> TimeToSampleTable;
    //    public BlobArray<SampleChunk> SampleToChunkTable;
    //    public BlobArray<ChunkOffset> ChunkOffsetTable;
    //}

    //public struct MP4Header
    //{
    //    public ulong Duration;
    //    public uint Timescale;

    //    public FileBlock DataBlock;

    //    public BlobArray<MP4VideoTrack> Videos;
    //    public BlobArray<MP4AudioTrack> Audios;
    //}

    public enum MP4Error
    {
        None,
        FileNotFound,
        InvalidFileStatus,
        InvalidHandle,
        InvalidBoxType,
        InvalidBoxSize,
        InvalidBoxVersion,
        InvalidReadSize,
        InvalidEntryCount,
        InvalidChromaFormat,
        InvalidBitDepth,
        OverMaxPolicy,
        DuplicateBox,
        MissingBox,
        IndexOutOfReaderRange,
        IllegalBoxDepth,
        ConflictingBitDepth
    }

    public enum VideoCodec
    {
        H264,
        H265
    }

    public struct MP4VideoDescription
    {
        public int ReferenceIndex;

        public VideoCodec CodecID;
        public uint CodecTag;

        public uint Width, Height;
        public int Depth;

        public ArrayBlock Extra;
    }

    public struct MP4AudioDescription
    {
        public int Samplerate;
        public int ChannelCount;
    }

    public struct MP4Context : IDisposable
    {
        public int BoxDepth;

        public ISODate CreationTime;
        public ISODate ModificationTime;

        public ulong Duration;
        public uint Timescale;
        public uint NextTrackID;

        public UnsafeList<MP4TrackContext> TrackList;
        public UnsafeList<MP4VideoDescription> VideoList;
        public UnsafeList<MP4AudioDescription> AudioList;

        public ref MP4TrackContext LastTrack =>
            ref TrackList.ElementAt(TrackList.Length - 1);

        public ref MP4VideoDescription LastVideo =>
            ref VideoList.ElementAt(VideoList.Length - 1);

        public ref MP4AudioDescription LastAudio =>
            ref AudioList.ElementAt(AudioList.Length - 1);

        public int Tag => GetTag(TrackList.Length);

        public int GetTag(int id) => (int)MP4Tag.Parse + TrackList.Length * 100 + id;

        public MP4Context(Allocator allocator)
        {
            BoxDepth = 0;

            CreationTime = 0;
            ModificationTime = 0;

            Duration = 0;
            Timescale = 0;
            NextTrackID = 0;

            TrackList = new UnsafeList<MP4TrackContext>(10, allocator);
            VideoList = new UnsafeList<MP4VideoDescription>(2, allocator);
            AudioList = new UnsafeList<MP4AudioDescription>(8, allocator);
        }

        public void Clear()
        {
            BoxDepth = 0;

            Duration = 0;
            Timescale = 0;
            NextTrackID = 0;

            TrackList.Clear();
            VideoList.Clear();
            AudioList.Clear();
        }

        public void Dispose()
        {
            TrackList.Dispose();
            VideoList.Dispose();
            AudioList.Dispose();
        }
    }

    public struct MP4TrackContext
    {
        public MP4Error Error;

        public ISOHandler Handler;
        public uint TrackID;
        public uint Timescale;
        public ulong Duration;
        public ISOLanguage Language;

        public SampleArray STTS;
        public SampleArray STSC;
        public SampleArray STCO;

        public ArrayBlock Descriptions;
    }

    public struct SampleArray
    {
        public int SampleIndex;
        public int EntryCount;
    }

    //public struct MP4ParseBlobJob : IJob
    //{
    //    public NativeReference<BByteReader> Reader;
    //    public NativeReference<JobLogger> Logger;
    //    public NativeReference<BlobAssetReference<MP4Header>> Header;
    //    public NativeReference<FileBlock> MDAT;

    //    public unsafe void Execute()
    //    {
    //        ref var reader = ref Reader.AsRef();
    //        ref var logger = ref Logger.AsRef();

    //        var context = new MP4Context();

    //        var moovBox = reader.ReadISOBox();
    //        Assert.AreEqual(ISOBoxType.MOOV, moovBox.type, "ISOBoxType");

    //        var error = ISOBMFF.Read(ref context, ref reader, ref logger, moovBox);

    //        var builder = new BlobBuilder(Allocator.Temp);
    //        ref var header = ref builder.ConstructRoot<MP4Header>();

    //        var videos = builder.Allocate(ref header.Videos, context.VideoList.Length);
    //        var audios = builder.Allocate(ref header.Audios, context.AudioList.Length);

    //        header.DataBlock = MDAT.Value;
    //        header.Duration = context.Duration;
    //        header.Timescale = context.Timescale;

    //        int videoCount = 0, audioCount = 0;
    //        for (int i = 0; i < context.TrackList.Length; i++)
    //        {
    //            ref var source = ref context.TrackList.ElementAt(i);

    //            switch (source.Handler)
    //            {
    //                case ISOHandler.VIDE:
    //                    {
    //                        ref var dest = ref videos[videoCount++];

    //                        dest.ID = source.TrackID;
    //                        dest.Duration = source.Duration;
    //                        dest.Timescale = source.Timescale;
    //                        dest.Language = source.Language;

    //                        dest.Width = source.wi;
    //                        dest.Heigth = source.he;

    //                        BuildTimeToSampleTable(ref reader, ref builder, ref dest.TimeToSampleTable, source.STTS);
    //                        BuildSampleToChunkTable(ref reader, ref builder, ref dest.SampleToChunkTable, source.STSC);
    //                        BuildChunkOffsetTable(ref reader, ref builder, ref dest.ChunkOffsetTable, source.STCO);
    //                    }
    //                    break;
    //                case ISOHandler.SOUN:
    //                    {
    //                        ref var dest = ref audios[audioCount++];

    //                        dest.ID = source.TrackID;
    //                        dest.Duration = source.Duration;
    //                        dest.Timescale = source.Timescale;
    //                        dest.Language = source.Language;

    //                        BuildTimeToSampleTable(ref reader, ref builder, ref dest.TimeToSampleTable, source.STTS);
    //                        BuildSampleToChunkTable(ref reader, ref builder, ref dest.SampleToChunkTable, source.STSC);
    //                        BuildChunkOffsetTable(ref reader, ref builder, ref dest.ChunkOffsetTable, source.STCO);
    //                    }
    //                    break;
    //            }
    //        }

    //        Header.Value = builder.CreateBlobAssetReference<MP4Header>(Allocator.Persistent);

    //        builder.Dispose();
    //        context.Dispose();
    //    }

    //    public static MP4Error BuildTimeToSampleTable(ref BByteReader reader, ref BlobBuilder builder, ref BlobArray<TimeSample> sttsArray, in SampleArray stts)
    //    {
    //        reader.Index = stts.SampleIndex;
    //        var array = builder.Allocate(ref sttsArray, stts.EntryCount);
    //        for (int i = 0; i < array.Length; i++)
    //        {
    //            array[i] = new TimeSample
    //            {
    //                count = reader.ReadUInt32(),
    //                delta = reader.ReadUInt32()
    //            };
    //        }

    //        return MP4Error.None;
    //    }

    //    public static MP4Error BuildSampleToChunkTable(ref BByteReader reader, ref BlobBuilder builder, ref BlobArray<SampleChunk> stscArray, in SampleArray stsc)
    //    {
    //        reader.Index = stsc.SampleIndex;
    //        var array = builder.Allocate(ref stscArray, stsc.EntryCount);
    //        for (int i = 0; i < array.Length; i++)
    //        {
    //            array[i] = new SampleChunk
    //            {
    //                firstChunk = reader.ReadUInt32(),
    //                samplesPerChunk = reader.ReadUInt32(),
    //                sampleDescriptionIndex = reader.ReadUInt32()
    //            };
    //        }

    //        return MP4Error.None;
    //    }

    //    public static MP4Error BuildChunkOffsetTable(ref BByteReader reader, ref BlobBuilder builder, ref BlobArray<ChunkOffset> stcoArray, in SampleArray stco)
    //    {
    //        reader.Index = stco.SampleIndex;
    //        var array = builder.Allocate(ref stcoArray, stco.EntryCount);
    //        for (int i = 0; i < array.Length; i++)
    //        {
    //            array[i] = new ChunkOffset
    //            {
    //                value = reader.ReadUInt32()
    //            };
    //        }

    //        return MP4Error.None;
    //    }
    //}
}
