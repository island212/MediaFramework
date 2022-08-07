using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MediaFramework.LowLevel.Unsafe
{
    /// <summary>
    /// Byte Reader (Big-Endian) 
    /// </summary>
    public unsafe struct BByteReader
    {
        [NativeDisableUnsafePtrRestriction]
        public byte* m_Head;

        [NativeDisableUnsafePtrRestriction]
        public readonly byte* m_Buffer;

        public readonly int m_Length;

        public bool IsInitialized => m_Head != null && m_Buffer != null && m_Length > 0;

        public bool IsValid => IsInitialized && m_Buffer <= m_Head;

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

        public BByteReader(void* buffer, int length)
        {
            m_Buffer = m_Head = (byte*)buffer;
            m_Length = length;
        }

        public BByteReader(in NativeArray<byte> array)
        {
            m_Buffer = m_Head = (byte*)array.GetUnsafeReadOnlyPtr();
            m_Length = array.Length;
        }

        public BByteReader(in NativeList<byte> list)
        {
            m_Buffer = m_Head = (byte*)list.GetUnsafeReadOnlyPtr();
            m_Length = list.Length;
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
                   (ulong)*m_Head++ << 24 | (ulong)*m_Head++ << 16 | (ulong)*m_Head++ << 8 | *m_Head++;
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
                   (long)*m_Head++ << 24 | (long)*m_Head++ << 16 | (long)*m_Head++ << 8 | *m_Head++;
        }

        public void Seek(int count)
        {
            m_Head += count;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public void CheckForValidRange(int index)
        {
            if (index < 0 || index > Length)
                throw new System.ArgumentOutOfRangeException();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public void CheckForOutOfRange(uint count)
        {
            if (Index + count > m_Length)
                throw new System.ArgumentOutOfRangeException();
        }
    }
}
