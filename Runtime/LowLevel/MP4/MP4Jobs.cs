using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using System;
using Unity.Burst;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Assertions;
using System.Runtime.ConstrainedExecution;
using static PlasticPipe.Server.MonitorStats;
using System.Diagnostics;

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

    public enum MediaCodec
    {
        Unspecified = 0,

        // Video Codecs
        H264,
        H265,
        AV1,

        // Audio Codecs
        PCM = 0x10000,
        AAC
    }

    public struct MP4Context : IDisposable
    {
        public Allocator Allocator;

        public int BoxDepth;
        public FileBlock MOOV, MDAT;

        public ISODate CreationTime;
        public ISODate ModificationTime;

        public ulong Duration;
        public uint Timescale;
        public uint NextTrackID;

        public UnsafeList<MP4TrackContext> TrackList;

        public UnsafeArray RawHeader;

        public ref MP4TrackContext LastTrack =>
            ref TrackList.ElementAt(TrackList.Length - 1);

        public int Tag => TrackList.Length * 1000;

        public MP4Context(Allocator allocator)
        {
            Allocator = allocator;

            BoxDepth = 0;
            MOOV = new FileBlock(); 
            MDAT = new FileBlock();
            CreationTime = new ISODate();
            ModificationTime = new ISODate();
            Duration = 0; 
            Timescale = 0; 
            NextTrackID = 0;
            RawHeader = new UnsafeArray();

            TrackList = new UnsafeList<MP4TrackContext>(8, allocator);
        }

        public void Dispose()
        {
            TrackList.Dispose();
            RawHeader.Dispose();
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

        public SampleArray<TimeSample> STTS;
        public SampleArray<SampleChunk> STSC;
        public SampleArray<ChunkOffset> STCO;
        // public SampleArray<ChunkOffset64> CO64;

        public MediaCodec Codec;
        public uint CodecTag;

        // Video
        public uint ReferenceIndex;
        public uint Width, Height;
        public int Depth;

        //public ColorPrimaries ColorPrimaries;
        //public ColorTransferCharacteristic ColorTransfer;
        //public ColorMatrix ColorMatrix;
        //public int FullRange;

        public UnsafeArray CodecExtra;

        // Audio
        public int SampleRate;
        public int ChannelCount;
    }

    public unsafe struct SampleArray<T> where T : unmanaged
    {
        public int Length;
        public T* Ptr;

        public SampleArray(int length, Allocator allocator)
        {
            Length = length;
            Ptr = (T*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * length,
                UnsafeUtility.AlignOf<T>(), allocator);
        }
    }

    [BurstCompile]
    public struct FindRootBoxJob : IJob
    {
        public ReadHandle ReadHandle;

        public UnsafeReference<AVIOContext> IOContext;
        public UnsafeReference<MP4Context> MP4Header;
        public UnsafeReference<JobLogger> Logger;

        public unsafe void Execute()
        {
            ref var avio = ref IOContext.GetReference();
            if (avio.Read.Size == 0)
                return;

            Assert.AreEqual(ReadStatus.Complete, ReadHandle.Status, "Invalid ReadHandle Status");

            ref var mp4 = ref MP4Header.GetReference();
            ref var logger = ref Logger.GetReference();

            var reader = new BByteReader(avio.Read.Buffer, (int)avio.Read.Size, Allocator.None);

            while (reader.Index + ISOBox.ByteNeeded < reader.Length)
            {
                var startOffset = reader.Index;

                var isoBox = reader.ReadISOBox();

                long size;
                if (isoBox.size >= ISOBox.ByteNeeded)
                    size = isoBox.size;
                else if (isoBox.size == 1)
                    size = (long)reader.ReadUInt64();
                else if (isoBox.size == 0)
                    size = avio.FileSize - avio.Read.Offset;
                else
                {
                    logger.LogError((int)MP4Tag.FindRootBox + 1, $"{MP4Error.InvalidBoxSize}: Invalid box size {isoBox.size} for {isoBox.type}");
                    avio.Read.Size = 0;
                    return;
                }

                logger.Trace(0, $"Start:{avio.Read.Offset + reader.Index} < End:{avio.Read.Offset + reader.Length} Type={isoBox.type} Size={size}");

                switch (isoBox.type)
                {
                    case ISOBoxType.MOOV:
                        if (mp4.MOOV.IsValid)
                        {
                            logger.LogError((int)MP4Tag.FindRootBox + 2, $"{MP4Error.DuplicateBox}: Duplicate {isoBox.type} box detected");
                            avio.Read.Size = 0;
                            return;
                        }

                        mp4.MOOV.Offset = avio.Read.Offset;
                        mp4.MOOV.Length = size;
                        break;
                    case ISOBoxType.MDAT:
                        if (mp4.MDAT.IsValid)
                        {
                            logger.LogError((int)MP4Tag.FindRootBox + 3, $"{MP4Error.DuplicateBox}: Duplicate {isoBox.type} box detected");
                            avio.Read.Size = 0;
                            return;
                        }

                        mp4.MDAT.Offset = avio.Read.Offset;
                        mp4.MDAT.Length = size;
                        break;
                }

                reader.Index = (int)math.min(size + startOffset, reader.Length);
                avio.Read.Offset += size;
            }

            avio.Read.Size = !mp4.MOOV.IsValid || !mp4.MDAT.IsValid
                ? math.min(avio.ReadBuffer.Length, math.max(avio.FileSize - avio.Read.Offset, 0)) : 0;

            ReadHandle.Dispose();
        }
    }

    [BurstCompile]
    public struct LoadMP4HeaderInMemoryJob : IJob
    {
        public UnsafeReference<AVIOContext> IOContext;
        public UnsafeReference<MP4Context> MP4Header;
        public UnsafeReference<JobLogger> Logger;

        public void Execute()
        {
            ref var mp4 = ref MP4Header.GetReference();
            ref var logger = ref Logger.GetReference();
            ref var avio = ref IOContext.GetReference();

            if (logger.Errors > 0 || !mp4.MOOV.IsValid || !mp4.MDAT.IsValid)
            {
                avio.Read.Size = 0;
                return;
            }

            if (mp4.MOOV.Length > int.MaxValue)
            {
                logger.LogError((int)MP4Tag.FindRootBox, $"{MP4Error.OverMaxPolicy}: MP4 header is more than int.MaxValue which is not supported. Size={mp4.MOOV.Length}");
                avio.Read.Size = 0;
                return;
            }

            mp4.RawHeader = new UnsafeArray((int)mp4.MOOV.Length, 4, Allocator.Persistent);

            avio.Read.Offset = mp4.MOOV.Offset;
            avio.Read.Size = mp4.MOOV.Length;
            unsafe { avio.Read.Buffer = mp4.RawHeader.Ptr; }
        }
    }

    [BurstCompile]
    public struct ParseMP4Job : IJob
    {
        public UnsafeReference<MP4Context> MP4Header;
        public UnsafeReference<JobLogger> Logger;

        public void Execute()
        {
            ref var mp4 = ref MP4Header.GetReference();
            ref var logger = ref Logger.GetReference();
        }
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
