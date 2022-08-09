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

    public struct MP4Validator : IDisposable
    {
        public UnsafeList<int> m_Groups;
        public UnsafeList<ValidatorLog> m_Logs;

        public int m_Length;
        public int m_CurrentGroup;

        public bool IsCreated => m_Groups.IsCreated && m_Logs.IsCreated;

        public int Length => m_Length;

        public bool HasError => m_Length > 0;

        public MP4Validator(int capacity, Allocator allocator)
        {
            m_Groups = new UnsafeList<int>(capacity, allocator);
            m_Logs = new UnsafeList<ValidatorLog>(capacity, allocator);

            m_Length = 0;
            m_CurrentGroup = 0;
        }

        public IEnumerable<ValidatorLog> GetLogs() => m_Logs;

        public void NewGroup()
        {
            m_CurrentGroup++;
        }

        public void Report(MP4Error error, FixedString128Bytes message)
        {
            m_Groups.Add(m_CurrentGroup);
            m_Logs.Add(new ValidatorLog
            {
                Error = error,
                Message = message
            });
        }

        public void Dispose()
        {
            m_Logs.Dispose();
        }
    }

    public unsafe struct MP4JobContext
    {
        public int BoxDepth;

        public MVHDBox MVHD;
        public UnsafeList<TRAKBox> Tracks;

        public JobLogger Logger;

        public ref TRAKBox CurrentTrack => ref Tracks.ElementAt(Tracks.Length - 1);

        public MP4Error LogError(MP4Error error, in FixedString128Bytes message)
        {
            Logger.Log(new JobLog
            {
                Type = LogType.Error,
                MetaData1 = Tracks.Length,
                MetaData2 = (int)error,
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
