using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

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

    public static class MP4Parser
    {
        //public unsafe static JobHandle Parse(string path, in MP4Policies policies, out NativeReference<MP4Context> context, JobHandle depends)
        //{
        //    context = new NativeReference<MP4Context>(Allocator.Persistent);

        //    ref var contextRef = ref context.AsRef();

        //    InitParser(ref contextRef, path);

        //    if (contextRef.Error != MP4Error.None)
        //        return depends;

        //    if (contextRef.MOOV.Length > policies.MaxMOOVSize)
        //    {
        //        contextRef.ReportError(MP4Error.OverMaxPolicy, (int)ISOBoxType.MOOV);
        //        return depends;
        //    }

        //    LoadMOOVInMemory(ref contextRef, path);

        //    return depends;
        //}

        //public unsafe static void InitParser(ref MP4Context context, string path)
        //{
        //    FileInfoResult infoResult;
        //    AsyncReadManager.GetFileInfo(path, &infoResult).JobHandle.Complete();

        //    if (infoResult.FileState == FileState.Absent)
        //    {
        //        context.ReportError(MP4Error.FileNotFound);
        //        return;
        //    }

        //    context.MOOV = FileBlock.Invalid;
        //    context.MDAT = FileBlock.Invalid;
        //    context.FileSize = infoResult.FileSize;
        //    context.Tracks = new UnsafeList<MP4Track>(4, Allocator.Persistent);

        //    using var discoveryBuffer = new NativeArray<byte>(4096, Allocator.Temp);

        //    ReadCommand readCommand;
        //    readCommand.Buffer = discoveryBuffer.GetUnsafePtr();
        //    readCommand.Size = discoveryBuffer.Length;
        //    readCommand.Offset = 0;

        //    var reader = new BByteReader(discoveryBuffer.GetUnsafePtr(), discoveryBuffer.Length);

        //    int boxesFound = 0;

        //    long size;
        //    while (readCommand.Offset < context.FileSize && boxesFound < 2)
        //    {
        //        using var handle = AsyncReadManager.Read(path, &readCommand, 1);
        //        handle.JobHandle.Complete();
        //        handle.Dispose();

        //        var isoBox = reader.PeekISOBox(0);

        //        if (isoBox.Size >= ISOBox.ByteNeeded)
        //            size = isoBox.Size;
        //        else if (isoBox.Size == 1)
        //            size = (long)BigEndian.ReadUInt64(reader.m_Head + ISOBox.ByteNeeded);
        //        else if (isoBox.Size == 0)
        //            size = context.FileSize - readCommand.Offset;
        //        else
        //        {
        //            context.ReportError(MP4Error.InvalidBoxSize, (int)isoBox.Type);
        //            return;
        //        }

        //        switch (isoBox.Type)
        //        {
        //            case ISOBoxType.MOOV:
        //                if (!context.MOOV.IsValid)
        //                {
        //                    context.MOOV.Offset = readCommand.Offset;
        //                    context.MOOV.Length = size;
        //                    boxesFound++;
        //                }
        //                else
        //                {
        //                    context.ReportError(MP4Error.DuplicateBox, (int)isoBox.Type);
        //                    return;
        //                }
        //                break;
        //            case ISOBoxType.MDAT:
        //                if (!context.MDAT.IsValid)
        //                {
        //                    context.MDAT.Offset = readCommand.Offset;
        //                    context.MDAT.Length = size;
        //                    boxesFound++;
        //                }
        //                else
        //                {
        //                    context.ReportError(MP4Error.DuplicateBox, (int)isoBox.Type);
        //                    return;
        //                }
        //                break;
        //        }

        //        readCommand.Offset += size;
        //    }
        //}

        //public unsafe static void LoadMOOVInMemory(ref MP4Context context, string path)
        //{
        //    context.MOOVData = (byte*)UnsafeUtility.Malloc(context.MOOV.Length, 4, Allocator.Persistent);

        //    ReadCommand readCommand;
        //    readCommand.Buffer = context.MOOVData;
        //    readCommand.Size = context.MOOV.Length;
        //    readCommand.Offset = context.MOOV.Offset;

        //    using var handle = AsyncReadManager.Read(path, &readCommand, 1);
        //    handle.JobHandle.Complete();
        //    handle.Dispose();
        //}
    }
}
