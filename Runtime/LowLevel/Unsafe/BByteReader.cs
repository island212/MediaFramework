using MediaFramework.LowLevel.MP4;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MediaFramework.LowLevel.Unsafe
{
    /// <summary>
    /// Byte Reader (Big-Endian) 
    /// </summary>
    [NativeContainer]
    public unsafe struct BByteReader : IDisposable
    {
        private readonly int m_Length;

        private readonly Allocator m_Allocator;

        [NativeDisableUnsafePtrRestriction]
        private byte* m_Head;

        [NativeDisableUnsafePtrRestriction]
        private readonly byte* m_Buffer;

        public bool IsCreated => m_Head != null && m_Buffer != null && m_Length > 0;

        public bool IsValid => IsCreated && m_Buffer <= m_Head;

        public int Index
        {
            get => (int)(m_Head - m_Buffer);
            set
            {
                CheckForValidRange(value);

                m_Head = m_Buffer + value;
            }
        }

        public int Length => m_Length;

        public int Remains => m_Length - Index;

        public BByteReader(void* ptr, int length, Allocator allocator)
        {
            m_Buffer = m_Head = (byte*)ptr;

            m_Length = length;
            m_Allocator = allocator;
        }

        public BByteReader(NativeList<byte> list, Allocator allocator)
        {
            m_Buffer = m_Head = (byte*)list.GetUnsafeReadOnlyPtr();

            m_Length = list.Length;
            m_Allocator = allocator;
        }

        public BByteReader(int length, Allocator allocator)
        {
            m_Buffer = m_Head = (byte*)UnsafeUtility.Malloc(length, 4, allocator);

            m_Length = length;
            m_Allocator = allocator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUInt8()
        {
            CheckForOutOfRange(1);

            return *m_Head++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            CheckForOutOfRange(2);

            return (ushort)(*m_Head++ << 8 | *m_Head++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt24()
        {
            CheckForOutOfRange(3);

            return (uint)(*m_Head++ << 16 | *m_Head++ << 8 | *m_Head++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            CheckForOutOfRange(4);

            return (uint)(*m_Head++ << 24 | *m_Head++ << 16 | *m_Head++ << 8 | *m_Head++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            CheckForOutOfRange(8);

            return (ulong)*m_Head++ << 56 | (ulong)*m_Head++ << 48 | (ulong)*m_Head++ << 40 | (ulong)*m_Head++ << 32 |
                   (ulong)*m_Head++ << 24 | (ulong)*m_Head++ << 16 | (ulong)*m_Head++ << 08 | *m_Head++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadInt8()
        {
            CheckForOutOfRange(1);

            return (sbyte)*m_Head++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            CheckForOutOfRange(2);

            return (short)(*m_Head++ << 8 | *m_Head++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt24()
        {
            CheckForOutOfRange(3);

            return *m_Head++ << 16 | *m_Head++ << 8 | *m_Head++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            CheckForOutOfRange(4);

            return *m_Head++ << 24 | *m_Head++ << 16 | *m_Head++ << 8 | *m_Head++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            CheckForOutOfRange(8);

            return (long)*m_Head++ << 56 | (long)*m_Head++ << 48 | (long)*m_Head++ << 40 | (long)*m_Head++ << 32 |
                   (long)*m_Head++ << 24 | (long)*m_Head++ << 16 | (long)*m_Head++ << 08 | *m_Head++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int count)
        {
            CheckForValidRange(Index + count);

            m_Head += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayBlock SeekArray(int length)
        {
            CheckForValidRange(Index + length);

            m_Head += length;
            return new ArrayBlock
            {
                Offset = Index - length,
                Length = length
            };
        }

        public void* GetUnsafePtr() => m_Buffer;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckForValidRange(int index)
        {
            if (!IsValid)
                throw new System.InvalidOperationException();

            if (index < 0 || index > Length)
                throw new System.ArgumentOutOfRangeException();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckForOutOfRange(int count)
        {
            if(!IsValid)
                throw new System.InvalidOperationException();

            if (Index + count > m_Length)
                throw new System.ArgumentOutOfRangeException();
        }

        public void Dispose()
        {
            if (m_Allocator == Allocator.Invalid || m_Allocator == Allocator.None)
                return;

            UnsafeUtility.Free(m_Buffer, m_Allocator);
            m_Head = null;
        }
    }
}
