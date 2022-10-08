using MediaFramework.LowLevel.Unsafe;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Assertions;
using Debug = UnityEngine.Debug;
using System.IO;
using Unity.Mathematics;
using UnityEngine.UIElements;
using System.Net;

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
        //public NativeReference<BlobAssetReference<MP4Header>> Header;
        public NativeReference<FileBlock> MDAT;

        public MP4ParseHandle(Allocator allocator)
        {
            Logger = new NativeReference<JobLogger>(allocator);
            Reader = new NativeReference<BByteReader>(allocator);
            //Header = new NativeReference<BlobAssetReference<MP4Header>>(allocator);

            MDAT = new NativeReference<FileBlock>(allocator);
            MDAT.Value = FileBlock.Invalid;
        }

        public void Dispose()
        {
            Logger.Dispose();
            Reader.Dispose();
            //Header.Dispose();
            MDAT.Dispose();
        }
    }

    public struct FindRootBox : IJob
    {
        public ReadHandle ReadHandle;
        public MediaHandle MediaHandle;

        [ReadOnly] public NativeArray<byte> Buffer;

        public unsafe void Execute()
        {
            ref var mp4 = ref MP4ParseSystem.AsRef(MediaHandle);
            if (mp4.Read.Size == 0)
                return;

            Assert.AreEqual(ReadStatus.Complete, ReadHandle.Status, "Invalid ReadHandle Status");

            var reader = new BByteReader(Buffer.GetUnsafeReadOnlyPtr(), (int)mp4.Read.Size, Allocator.None);

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
                    size = mp4.FileSize - mp4.Read.Offset;
                else
                {
                    mp4.Logger.LogError((int)MP4Tag.ReadHeader + 1, $"{MP4Error.InvalidBoxSize}: Invalid box size {isoBox.size} for {isoBox.type}");
                    mp4.Read.Size = 0;
                    return;
                }

                mp4.Logger.Trace(0, $"Start:{mp4.Read.Offset + reader.Index} < End:{mp4.Read.Offset + reader.Length} Type={isoBox.type} Size={size}");

                switch (isoBox.type)
                {
                    case ISOBoxType.MOOV:
                        if (mp4.MOOV.IsValid)
                        {
                            mp4.Logger.LogError((int)MP4Tag.ReadHeader + 2, $"{MP4Error.DuplicateBox}: Duplicate {isoBox.type} box detected");
                            mp4.Read.Size = 0;
                            return;
                        }

                        mp4.MOOV.Offset = mp4.Read.Offset;
                        mp4.MOOV.Length = size;
                        break;
                    case ISOBoxType.MDAT:
                        if (mp4.MDAT.IsValid)
                        {
                            mp4.Logger.LogError((int)MP4Tag.ReadHeader + 3, $"{MP4Error.DuplicateBox}: Duplicate {isoBox.type} box detected");
                            mp4.Read.Size = 0;
                            return;
                        }

                        mp4.MDAT.Offset = mp4.Read.Offset;
                        mp4.MDAT.Length = size;
                        break;
                }

                // In case, the size is bigger than int.MaxValue (~4GB which can happen) 
                reader.Index = (int)math.min(size + startOffset, reader.Length);
                mp4.Read.Offset += size;
            }

            mp4.Read.Size = !mp4.MOOV.IsValid || !mp4.MDAT.IsValid
                ? math.min(Buffer.Length, math.max(mp4.FileSize - mp4.Read.Offset, 0)) : 0;

            ReadHandle.Dispose();
        }
    }


    public enum MP4Tag
    {
        ReadHeader = 10000,
        Parse = 20000,
        AVCCRead = Parse + 132
    }

    public enum MediaType
    { 
        Unknown = 0,
        MP4
    }

    public unsafe readonly struct MediaHandle
    {
        public static MediaHandle Invalid => new MediaHandle();

        public MediaType Type => m_Type;

        [NativeDisableUnsafePtrRestriction]
        internal readonly IntPtr m_Data;

        internal readonly MediaType m_Type;

        internal MediaHandle(MediaType type, void* ptr)
        {
            m_Type = type;
            m_Data = new IntPtr(ptr);
        }
    }

    public struct MP4File
    {
        public FileHandle File;
        public long FileSize;

        public ReadCommand Read;

        public FileBlock MOOV, MDAT;

        public JobLogger Logger;
    }

    public struct MP4ParseSystem
    {
        internal int NextID;

        internal NativeList<IntPtr> FileHandles;

        public void OnCreate(int size = 8)
        {
            NextID = 1;

            FileHandles = new NativeList<IntPtr>(size, Allocator.Persistent);
        }

        public void OnDestroy()
        {
            unsafe
            {
                foreach (var handle in FileHandles)
                {
                    var ptr = (MP4File*)handle.ToPointer();

                    ptr->File.Close();
                    ptr->Logger.Dispose();

                    UnsafeUtility.Free(ptr, Allocator.Persistent);
                }
            }

            FileHandles.Dispose();
        }

        public MediaHandle Open(string path)
        {
            FileInfoResult infoResult;

            unsafe
            {
                var infoHandle = AsyncReadManager.GetFileInfo(path, &infoResult);
                infoHandle.JobHandle.Complete();
            }

            if (infoResult.FileState == FileState.Absent)
            {
                Debug.LogError($"{MP4Error.FileNotFound}: File not found at path {path}");
                return MediaHandle.Invalid;
            }

            var file = AsyncReadManager.OpenFileAsync(path);
            file.JobHandle.Complete();

            return CreateNewHandle(file, infoResult.FileSize);
        }

        public JobHandle Prepare(MediaHandle handle, JobHandle depends = default)
        {
            ref var media = ref AsRef(handle);

            var status = media.File.Status;
            if (status == FileStatus.Pending)
            {
                media.File.JobHandle.Complete();
                status = media.File.Status;
            }

            switch (status)
            {
                case FileStatus.Closed:
                    media.Logger.LogError((int)MP4Tag.ReadHeader + 1, $"{MP4Error.InvalidFileStatus}: Failed to {nameof(MP4ParseSystem)}.{nameof(Prepare)}. Status={status}");
                    return depends;
                case FileStatus.OpenFailed:
                    media.Logger.LogError((int)MP4Tag.ReadHeader + 2, $"{MP4Error.InvalidFileStatus}: Failed to {nameof(MP4ParseSystem)}.{nameof(Prepare)}. Status={status}");
                    return depends;
            }

            const int length = 4096;

            var buffer = new NativeArray<byte>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            unsafe
            {
                media.Read.Offset = 0;
                media.Read.Size = math.min(media.FileSize, length);
                media.Read.Buffer = buffer.GetUnsafePtr();

                var readCommands = new ReadCommandArray();
                readCommands.CommandCount = 1;
                readCommands.ReadCommands = &GetUnsafePtr(handle)->Read;

                for (int i = 0; i < 20; i++)
                {
                    var readHandle = AsyncReadManager.ReadDeferred(media.File, &readCommands, depends);

                    // Weirdly the ReadDeferred dependency is not enough.
                    depends = JobHandle.CombineDependencies(readHandle.JobHandle, depends);

                    depends = new FindRootBox {
                        ReadHandle = readHandle,
                        MediaHandle = handle,
                        Buffer = buffer
                    }.Schedule(depends);
                }

                depends = buffer.Dispose(depends);
            }

            return depends;
        }

        public JobHandle CreateDemuxer(MediaHandle handle, JobHandle depends = default)
        {
            return depends;
        }

        public JobHandle Release(MediaHandle handle, JobHandle depends = default)
        {
            return depends;
        }

        public ref JobLogger GetLogs(MediaHandle handle) => ref AsRef(handle).Logger;

        public static unsafe ref MP4File AsRef(MediaHandle handle)
        {
            Assert.IsTrue(handle.m_Data != null && handle.m_Type == MediaType.MP4,
                $"{MP4Error.InvalidHandle}: Invalid handle argument for {nameof(MP4ParseSystem)}.{nameof(AsRef)}");

            return ref UnsafeUtility.AsRef<MP4File>(handle.m_Data.ToPointer());
        }

        public static unsafe MP4File* GetUnsafePtr(MediaHandle handle)
        {
            Assert.IsTrue(handle.m_Data != null && handle.m_Type == MediaType.MP4,
                $"{MP4Error.InvalidHandle}: Invalid handle argument for {nameof(MP4ParseSystem)}.{nameof(GetUnsafePtr)}");

            return (MP4File*)handle.m_Data.ToPointer();
        }

        internal unsafe MediaHandle CreateNewHandle(in FileHandle file, long fileSize)
        {
            var media = (MP4File*)UnsafeUtility.Malloc(sizeof(MP4File), 4, Allocator.Persistent);
            var handle = new MediaHandle(MediaType.MP4, media);

            UnsafeUtility.MemClear(media, sizeof(MP4File));

            media->File = file;
            media->FileSize = fileSize;
            media->MOOV = FileBlock.Invalid;
            media->MDAT = FileBlock.Invalid;

            media->Logger = new JobLogger(16, Allocator.Persistent);

            FileHandles.Add(handle.m_Data);

            return handle;
        }

        public unsafe static JobHandle Parse(string path, MP4ParseHandle input, JobHandle depends)
        {
            var handle = ScheduleInit(path, input, depends);

            //handle = new MP4ParseBlobJob
            //{
            //    Reader = input.Reader,
            //    Logger = input.Logger,
            //    Header = input.Header,
            //    MDAT = input.MDAT
            //}.Schedule(handle);
            return handle;
        }

        public unsafe static JobHandle ScheduleInit(string path, MP4ParseHandle input, JobHandle depends)
        {
            FileInfoResult infoResult;
            AsyncReadManager.GetFileInfo(path, &infoResult).JobHandle.Complete();

            ref var logger = ref input.Logger.AsRef();

            if (infoResult.FileState == FileState.Absent)
            {
                logger.LogError((int)MP4Tag.ReadHeader, $"{MP4Error.FileNotFound}: File not found at path {path}");
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
                    logger.LogError((int)MP4Tag.ReadHeader, $"{MP4Error.InvalidBoxSize}: Invalid box size {isoBox.size} for {isoBox.type}");
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
                                logger.LogError((int)MP4Tag.ReadHeader, $"{MP4Error.DuplicateBox}: Duplicate {ISOBoxType.MOOV} box detected");
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
                                logger.LogError((int)MP4Tag.ReadHeader, $"{MP4Error.DuplicateBox}: Duplicate {ISOBoxType.MDAT} box detected");
                                return depends;
                            }
                        }
                        break;
                }

                readCommand.Offset += size;
            }

            if (reader.IsCreated)
                logger.LogError((int)MP4Tag.ReadHeader, $"{MP4Error.MissingBox}: Missing MDAT box");
            else
                logger.LogError((int)MP4Tag.ReadHeader, $"{MP4Error.MissingBox}: Missing MOOV box");

            return depends;
        }
    }
}
