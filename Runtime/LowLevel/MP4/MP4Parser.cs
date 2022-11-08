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
using Unity.Burst;
using Unity.Entities;
using static CodiceApp.EventTracking.EventModelSerialization;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace MediaFramework.LowLevel.MP4
{
    public struct FileBlock : IEquatable<FileBlock>
    {
        public static FileBlock Invalid => new FileBlock();

        public bool IsValid => Offset >= 0 && Length > 0;

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

    public enum MP4Tag
    {
        FindRootBox = 10000,
        Parse = 20000,
        AVCCRead = Parse + 132
    }

    public enum MediaType
    { 
        Unknown = 0,
        MP4
    }

    public readonly struct MediaHandle : IEquatable<MediaHandle>
    {
        public static MediaHandle Invalid => new MediaHandle();

        internal readonly int Index;
        internal readonly int Version;

        internal MediaHandle(int index, int version)
        {
            Index = index;
            Version = version;
        }

        public bool Equals(MediaHandle other) => this == other;

        public static bool operator ==(MediaHandle handle1, MediaHandle handle2) 
            => handle1.Index == handle2.Index && handle1.Version == handle2.Version;

        public static bool operator !=(MediaHandle handle1, MediaHandle handle2) => !(handle1 == handle2);

        public override int GetHashCode() => HashCode.Combine(Index, Version);

        public override bool Equals(object obj) => obj is MediaHandle other && this == other;
    }

    public struct AVIOContext
    {
        public FileHandle File;
        public long FileSize;

        public ReadCommandArray Commands;
        public ReadCommand Read;
        public UnsafeArray ReadBuffer;
    }

    public unsafe struct UnsafeArray
    {
        public Allocator Allocator;
        public void* Ptr;
        public int Length;

        public UnsafeArray(void* ptr, int length, Allocator allocator)
        {
            Assert.AreNotEqual(Allocator.Invalid, allocator, "UnsafeArray can't be allocated using Invalid");

            Ptr = ptr;
            Length = length;
            Allocator = allocator;
        }

        public UnsafeArray(int length, int alignment, Allocator allocator)
        {
            Ptr = UnsafeUtility.Malloc(length, alignment, allocator);
            Length = length;
            Allocator = allocator;
        }

        public void Dispose()
        {
            if (Allocator != Allocator.Invalid && Allocator != Allocator.None && Ptr != null)
                UnsafeUtility.Free(Ptr, Allocator);

            Allocator = Allocator.Invalid;
            Ptr = null;
            Length = 0;
        }
    }

    public unsafe struct UnsafeReference<T> where T : struct
    {
        [NativeDisableUnsafePtrRestriction]
        public void* m_Data;

        public ref T GetReference() => ref UnsafeUtility.AsRef<T>(m_Data);

        public UnsafeReference(ref T data)
        {
            m_Data = UnsafeUtility.AddressOf(ref data);
        }

        public UnsafeReference(void* data)
        {
            m_Data = data;
        }
    }

    [BurstCompile]
    public struct MP4ParseSystem
    {
        internal NativeList<int> m_Versions;
        internal NativeList<MediaType> m_MediaTypes;
        internal NativeList<IntPtr> m_FileHeaders;
        internal NativeList<AVIOContext> m_MediaIOs;

        internal NativeList<JobLogger> m_Loggers;

        public void OnCreate(int size = 8)
        {
            m_Versions = new NativeList<int>(size, Allocator.Persistent);
            m_MediaIOs = new NativeList<AVIOContext>(size, Allocator.Persistent);
            m_MediaTypes = new NativeList<MediaType>(size, Allocator.Persistent);
            m_FileHeaders = new NativeList<IntPtr>(size, Allocator.Persistent);
            m_Loggers = new NativeList<JobLogger>(size, Allocator.Persistent);
        }

        public void OnDestroy()
        {
            unsafe
            {
                for (int i = 0; i < m_FileHeaders.Length; i++)
                {
                    var ptr = (MP4Context*)m_FileHeaders[i].ToPointer();
                    ptr->Dispose();
                }
            }

            for (int i = 0; i < m_MediaIOs.Length; i++)
            {
                m_MediaIOs[i].File.Close();
                m_MediaIOs[i].ReadBuffer.Dispose();
            }

            for (int i = 0; i < m_Loggers.Length; i++)
            {
                m_Loggers[i].Dispose();
            }

            m_Versions.Dispose();
            m_MediaIOs.Dispose();
            m_MediaTypes.Dispose();
            m_FileHeaders.Dispose();
            m_Loggers.Dispose();
        }

        [NotBurstCompatible]
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

            var mediaType = MediaType.Unknown;
            // TODO: Check the file signature if no extension
            switch (Path.GetExtension(path))
            {
                case ".mp4":
                    mediaType = MediaType.MP4;
                    break;
                default:
                    Debug.LogError($"{MP4Error.FileNotFound}: File not found at path {path}");
                    return MediaHandle.Invalid;
            }

            var file = AsyncReadManager.OpenFileAsync(path);
            file.JobHandle.Complete();

            switch (file.Status)
            {
                case FileStatus.Pending:
                    Debug.LogError("InvalidFileStatus: File was Pending");
                    return MediaHandle.Invalid;
                case FileStatus.Closed:
                    Debug.LogError("InvalidFileStatus: File was Closed");
                    return MediaHandle.Invalid;
                case FileStatus.OpenFailed:
                    Debug.LogError("InvalidFileStatus: File was OpenFailed");
                    return MediaHandle.Invalid;
            }

            return CreateNewHandle(mediaType, file, infoResult.FileSize);
        }

        public JobHandle ParseMP4(MediaHandle handle, JobHandle depends = default)
        {
            if (!IsValid(handle))
            {
                Debug.LogError("InvalidHandle: Handle is invalid or was closed");
                return default;
            }

            ref var avio = ref m_MediaIOs.ElementAt(handle.Index);
            ref var logger = ref m_Loggers.ElementAt(handle.Index);

            var status = avio.File.Status;
            if (status == FileStatus.Pending)
            {
                avio.File.JobHandle.Complete();
                status = avio.File.Status;
            }

            switch (status)
            {
                case FileStatus.Closed:
                    logger.LogError((int)MP4Tag.FindRootBox + 1, $"InvalidFileStatus: File was Closed");
                    return depends;
                case FileStatus.OpenFailed:
                    logger.LogError((int)MP4Tag.FindRootBox + 2, $"InvalidFileStatus: File was OpenFailed");
                    return depends;
            }

            unsafe
            {
                avio.ReadBuffer = new UnsafeArray(4096, 4, Allocator.TempJob);

                avio.Read.Offset = 0;
                avio.Read.Size = math.min(avio.FileSize, avio.ReadBuffer.Length);
                avio.Read.Buffer = avio.ReadBuffer.Ptr;

                var avioPtr = (AVIOContext*)UnsafeUtility.AddressOf(ref avio);
                var loggerPtr = (JobLogger*)UnsafeUtility.AddressOf(ref logger);
                var mp4Ptr = (MP4Context*)m_FileHeaders.ElementAt(handle.Index).ToPointer();

                avio.Commands.CommandCount = 1;
                avio.Commands.ReadCommands = &avioPtr->Read;

                ReadHandle readHandle;
                // We try 20 jump to find the MOOV and MDAT
                // TODO: Implement the case where 20 jumps is not enough
                for (int i = 0; i < 20; i++)
                {
                    readHandle = AsyncReadManager.ReadDeferred(avio.File, &avioPtr->Commands, depends);

                    // Weirdly the ReadDeferred dependency is not enough.
                    depends = JobHandle.CombineDependencies(readHandle.JobHandle, depends);

                    depends = new FindRootBoxJob {
                        ReadHandle = readHandle,
                        IOContext = new UnsafeReference<AVIOContext>(avioPtr),
                        MP4Header = new UnsafeReference<MP4Context>(mp4Ptr),
                        Logger = new UnsafeReference<JobLogger>(loggerPtr)
                    }.Schedule(depends);
                }

                depends = new LoadMP4HeaderInMemoryJob {
                    IOContext = new UnsafeReference<AVIOContext>(avioPtr),
                    MP4Header = new UnsafeReference<MP4Context>(mp4Ptr),
                    Logger = new UnsafeReference<JobLogger>(loggerPtr)
                }.Schedule(depends);

                readHandle = AsyncReadManager.ReadDeferred(avio.File, &avioPtr->Commands, depends);

                depends = JobHandle.CombineDependencies(readHandle.JobHandle, depends);

                depends = new ParseMP4Job {
                    ReadHandle = readHandle,
                    IOContext = new UnsafeReference<AVIOContext>(avioPtr),
                    MP4Header = new UnsafeReference<MP4Context>(mp4Ptr),
                    Logger = new UnsafeReference<JobLogger>(loggerPtr)
                }.Schedule(depends);
            }

            return depends;
        }

        public JobHandle Release(MediaHandle handle, JobHandle depends = default)
        {
            return depends;
        }

        public bool IsValid(MediaHandle handle)
        {
            return handle.Index >= 0 && handle.Index < m_Versions.Length && m_Versions[handle.Index] == handle.Version;
        }

        static MP4Context internal_MP4InvalidHeader = new MP4Context();

        public unsafe ref readonly MP4Context GetHeader(MediaHandle handle)
        {
            Assert.IsTrue(IsValid(handle), "InvalidHandle: Handle is invalid or was closed");

            if (m_MediaTypes[handle.Index] != MediaType.MP4)
            {
                Debug.LogError("InvalidMediaType: Handle is not an MP4");
                return ref internal_MP4InvalidHeader;
            }

            return ref UnsafeUtility.AsRef<MP4Context>(m_FileHeaders[handle.Index].ToPointer());
        }

        public ref readonly JobLogger GetLogs(MediaHandle handle)
        { 
            Assert.IsTrue(IsValid(handle), "InvalidHandle: Handle is invalid or was closed");

            return ref m_Loggers.ElementAt(handle.Index);
        }

        public ref readonly AVIOContext GetAVIOContext(MediaHandle handle)
        {
            Assert.IsTrue(IsValid(handle), "InvalidHandle: Handle is invalid or was closed");

            return ref m_MediaIOs.ElementAt(handle.Index);
        }

        public void PrintAll()
        {
            foreach (var logger in m_Loggers)
                logger.PrintAll();
        }

        internal unsafe MediaHandle CreateNewHandle(MediaType mediaType, FileHandle file, long fileSize)
        {
            var headerPtr = IntPtr.Zero;
            switch (mediaType)
            {
                case MediaType.MP4:
                    var ptr = (MP4Context*)UnsafeUtility.Malloc(sizeof(MP4Context), UnsafeUtility.AlignOf<MP4Context>(), Allocator.Persistent);
                    *ptr = new MP4Context(Allocator.Persistent);
                    headerPtr = new IntPtr(ptr);
                    break;
                default:
                    return MediaHandle.Invalid;
            }

            Assert.AreNotEqual(IntPtr.Zero, headerPtr, "Header pointer was null during CreateNewHandle");

            int index = m_Versions.Length;

            m_Versions.Add(1);
            m_MediaTypes.Add(mediaType);
            m_FileHeaders.Add(headerPtr);
            m_Loggers.Add(new JobLogger(16, Allocator.Persistent));
            m_MediaIOs.Add(new AVIOContext { 
                File = file,
                FileSize = fileSize
            });

            return new MediaHandle(index, 1);
        }

        internal unsafe void* ClearMalloc(long size, int alignment, Allocator allocator)
        {
            var ptr = UnsafeUtility.Malloc(size, alignment, allocator);
            UnsafeUtility.MemClear(ptr, size);
            return ptr;
        }
    }
}
