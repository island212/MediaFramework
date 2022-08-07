using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using UnityEngine;
using System;

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
        OverMaxPolicy,
        DuplicateBox,
        IndexOutOfReaderRange,

    }

    public enum Validation
    {
        Soft,
        Strict
    }

    public struct MP4Validator : IDisposable
    {
        public Validation Policy;
        public UnsafeStream Stream;
        public UnsafeList<int> Groups;
        public UnsafeList<MP4Error> Errors;

        public UnsafeStream.Writer Logger;

        public int CurrentGroup;

        public bool HasError => !Errors.IsEmpty;

        public MP4Validator(Validation policy, Allocator allocator)
        {
            Policy = policy;
            Stream = new UnsafeStream(1, allocator);
            Groups = new UnsafeList<int>(16, allocator);
            Errors = new UnsafeList<MP4Error>(16, allocator);

            Logger = Stream.AsWriter();

            CurrentGroup = 0;
        }

        public void NewGroup()
        {
            CurrentGroup++;
        }

        public void Report(MP4Error error, FixedString128Bytes message)
        {
            Groups.Add(CurrentGroup);
            Errors.Add(error);
            Logger.Write(message);
        }

        public void Dispose()
        {
            Stream.Dispose();
            Groups.Dispose();
            Errors.Dispose();
        }
    }

    public unsafe struct MP4JobContext
    {
        public BByteReader Reader;
        public MP4Validator Validator;

        public int TrackIndex;

        public Validation Policy => Validator.Policy;

        public MP4Error LogError(MP4Error error, in FixedString128Bytes message)
        {
            Validator.Report(error, $"{error}: {message}");
            return error;
        }

        public MP4Error LogTrackError(MP4Error error, in FixedString128Bytes message)
        {
            if (TrackIndex > 0)
                Validator.Report(error, $"{error}: Track#{TrackIndex} - {message}");
            else
                Validator.Report(error, $"{error}: Invalid Track - {message}");

            return error;
        }

        public MP4Error ValidateBox(in ISOBox box)
        {
            if(box.Size < ISOBox.ByteNeeded)
                return LogError(MP4Error.InvalidBoxSize, $"The {box.Type} box size is {box.Size} but need to be greater or equal to 8");

            if (Reader.Index + box.Size - ISOBox.ByteNeeded > Reader.Length)
                return LogError(MP4Error.IndexOutOfReaderRange, $"The {box.Type} box size is {box.Size} and was moving outside the reader buffer. Index={Reader.Index}, Length={Reader.Length}");

            return MP4Error.None;
        }

        public MP4Error ValidateFullBox(in ISOBox box, int size)
        {
            if (box.Size == size)
                return LogError(MP4Error.InvalidBoxSize, $"The {box.Type} box size is {box.Size} but can only be {size}");

            return ValidateBox(box);
        }

        public MP4Error ValidateFullBox(in ISOBox box, int version0, int version1)
        {
            if (box.Size != version0 && box.Size != version1)
                return LogError(MP4Error.InvalidBoxSize, $"The {box.Type} box size is {box.Size} but can only be either {version0} for version 0 or {version1} for version 1");

            return ValidateBox(box);
        }
    }

    public struct MP4ParseJob : IJob
    {
        public BByteReader Reader;
        public MP4Validator Validator;

        public NativeReference<BlobAssetReference<MP4Header>> Header;

        public unsafe void Execute()
        {
            var context = new MP4JobContext();
            context.Reader = Reader;
            context.Validator = Validator;
            context.TrackIndex = -1;

            var moovBox = Reader.ReadISOBox();

            MP4BoxUtility.CheckForBoxType(moovBox.Type, ISOBoxType.MOOV);

            var tracks = new UnsafeList<TRAKBox>(4, Allocator.Temp);

            int index = Reader.Index;
            while (index < Reader.Length)
            {
                var isoBox = Reader.ReadISOBox();

                switch (isoBox.Type)
                {
                    case ISOBoxType.TRAK:
                        context.TrackIndex = tracks.Length;
                        context.Validator.NewGroup();
                        tracks.Add(new TRAKBox());

                        TRAKBox.Read(ref context, ref tracks.ElementAt(context.TrackIndex), isoBox);
                        break;
                    default:
                        Reader.Seek((int)isoBox.Size - ISOBox.ByteNeeded);
                        break;
                }

                MP4BoxUtility.CheckIfReaderRead(index, context.Reader, isoBox);

                index = Reader.Index;
            }

            if (context.Policy == Validation.Strict && context.Validator.HasError)
                return;

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var header = ref builder.ConstructRoot<MP4Header>();

            var videos = builder.Allocate(ref header.Videos, 2);
        }
    }
}
