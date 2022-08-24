using log4net.Repository.Hierarchy;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UIElements;
using static CodiceApp.EventTracking.EventModelSerialization;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace MediaFramework.LowLevel.MP4
{
    public struct FileBlock : IEquatable<FileBlock>
    {
        public static FileBlock Invalid => new FileBlock(-1, -1);

        public bool IsValid => Offset >= 0 && Length >= 0;

        public long Offset;
        public long Length;

        public FileBlock(long offset, long length)
        {
            Offset = offset;
            Length = length;
        }

        public bool Equals(FileBlock other)
        {
            return Offset == other.Offset && Length == other.Length;
        }
    }

    public struct ArrayBlock : IEquatable<ArrayBlock>
    {
        public static ArrayBlock Invalid => new ArrayBlock(-1, -1);

        public int Offset;
        public int Length;

        public ArrayBlock(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        public bool Equals(ArrayBlock other)
        {
            return Offset == other.Offset && Length == other.Length;
        }
    }

    public struct MP4ParseHandle : IDisposable
    {
        public NativeReference<JobLogger> Logger;
        public NativeReference<BByteReader> Reader;
        public NativeReference<BlobAssetReference<MP4Header>> Header;
        public NativeReference<FileBlock> MDAT;

        public MP4ParseHandle(Allocator allocator)
        {
            Logger = new NativeReference<JobLogger>(allocator);
            Reader = new NativeReference<BByteReader>(allocator);
            Header = new NativeReference<BlobAssetReference<MP4Header>>(allocator);

            MDAT = new NativeReference<FileBlock>(allocator);
            MDAT.Value = FileBlock.Invalid;
        }

        public void Dispose()
        {
            Logger.Dispose();
            Reader.Dispose();
            Header.Dispose();
            MDAT.Dispose();
        }
    }

    public enum MP4Tag
    {
        Init = 10000,
        Parse = 20000,
        AVCCRead = Parse + 132
    }

    public static class MP4Parser
    {
        public unsafe static JobHandle Parse(string path, MP4ParseHandle input, JobHandle depends)
        {
            var handle = ScheduleInit(path, input, depends);

            handle = new MP4ParseBlobJob
            {
                Reader = input.Reader,
                Logger = input.Logger,
                Header = input.Header,
                MDAT = input.MDAT
            }.Schedule(handle);
            return handle;
        }

        public unsafe static JobHandle ScheduleInit(string path, MP4ParseHandle input, JobHandle depends)
        {
            FileInfoResult infoResult;
            AsyncReadManager.GetFileInfo(path, &infoResult).JobHandle.Complete();

            ref var logger = ref input.Logger.AsRef();

            if (infoResult.FileState == FileState.Absent)
            {
                logger.LogError((int)MP4Tag.Init, $"{MP4Error.FileNotFound}: File not found at path {path}");
                return depends;
            }

            using var discoveryBuffer = new NativeArray<byte>(4096, Allocator.Temp);

            ref var reader = ref input.Reader.AsRef();

            ReadCommand readCommand;
            readCommand.Buffer = discoveryBuffer.GetUnsafePtr();
            readCommand.Size = discoveryBuffer.Length;
            readCommand.Offset = 0;

            int boxesFound = 0;

            while (readCommand.Offset < infoResult.FileSize)
            {
                using var handle = AsyncReadManager.Read(path, &readCommand, 1);
                handle.JobHandle.Complete();
                handle.Dispose();

                var isoBox = reader.ReadISOBox();

                long size;
                if (isoBox.size >= ISOBox.ByteNeeded)
                {
                    size = isoBox.size;
                }
                else if (isoBox.size == 1)
                {
                    size = (long)reader.ReadUInt64();
                }
                else if (isoBox.size == 0)
                {
                    size = infoResult.FileSize - readCommand.Offset;
                }
                else
                {
                    logger.LogError((int)MP4Tag.Init, $"{MP4Error.InvalidBoxSize}: Invalid box size {isoBox.size} for {isoBox.type}");
                    return depends;
                }

                switch (isoBox.type)
                {
                    case ISOBoxType.MOOV:
                        {
                            if (!reader.IsCreated)
                            {
                                //FIXME: Check the size is not over filesize
                                reader = new BByteReader((int)isoBox.size, Allocator.TempJob);

                                ReadCommand readerCommand;
                                readerCommand.Buffer = reader.GetUnsafePtr();
                                readerCommand.Size = reader.Length;
                                readerCommand.Offset = readCommand.Offset;

                                using var readHandle = AsyncReadManager.Read(path, &readerCommand, 1);
                                readHandle.JobHandle.Complete();
                                readHandle.Dispose();

                                if (++boxesFound < 2)
                                    return depends;
                            }
                            else
                            {
                                logger.LogError((int)MP4Tag.Init, $"{MP4Error.DuplicateBox}: Duplicate {ISOBoxType.MOOV} box detected");
                                return depends;
                            }
                        }
                        break;
                    case ISOBoxType.MDAT:
                        {
                            if (!input.MDAT.Value.IsValid)
                            {
                                input.MDAT.Value = new FileBlock(readCommand.Offset, size);
                                if (++boxesFound < 2)
                                    return depends;
                            }
                            else
                            {
                                logger.LogError((int)MP4Tag.Init, $"{MP4Error.DuplicateBox}: Duplicate {ISOBoxType.MDAT} box detected");
                                return depends;
                            }
                        }
                        break;
                }

                readCommand.Offset += size;
            }

            if (reader.IsCreated)
                logger.LogError((int)MP4Tag.Init, $"{MP4Error.MissingBox}: Missing MDAT box");
            else
                logger.LogError((int)MP4Tag.Init, $"{MP4Error.MissingBox}: Missing MOOV box");

            return depends;
        }
    }
}
