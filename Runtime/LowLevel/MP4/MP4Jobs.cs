using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace MediaFramework.LowLevel.MP4
{
    public struct MP4VideoTrack
    {
        public uint Duraton;
        public uint TimeScale;
        public ISOLanguage Language;

        public BlobArray<byte> STTS;

        //public SequenceParameterSet SPS;
    }

    public struct MP4AudioTrack
    {
        public uint Duraton;
        public uint TimeScale;
        public ISOLanguage Language;

        public BlobArray<byte> STTS;
    }

    public struct MP4Header
    {
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

    public struct ValidatorLog
    {
        public MP4Error Error;
        public FixedString128Bytes Message;
    }

    public unsafe struct MP4JobContext
    {
        public int BoxDepth;

        public MVHDBox MVHD;
        public UnsafeList<TRAKBox> Tracks;

        public JobLogger Logger;

        public ref TRAKBox CurrentTrack => ref Tracks.ElementAt(Tracks.Length - 1);

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

    public struct MP4ParseJob : IJob
    {
        public MP4Policy Policy;
        public BByteReader Reader;

        public NativeReference<BlobAssetReference<MP4Header>> Header;

        public unsafe void Execute()
        {
            var context = new MP4JobContext();
            context.Logger = new JobLogger(16, Allocator.TempJob);
            context.Tracks = new UnsafeList<TRAKBox>(4, Allocator.Temp);

            var moovBox = Reader.ReadISOBox();

            ISOBMFF.Read(ref context, ref Reader, moovBox);

            if (Policy == MP4Policy.Strict)
                goto clean;

            var builder = new BlobBuilder(Allocator.Temp);
            ref var header = ref builder.ConstructRoot<MP4Header>();

            var videos = builder.Allocate(ref header.Videos, 2);

            builder.Dispose();

        clean:
            context.Tracks.Dispose();
        }
    }
}
