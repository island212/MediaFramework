using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MediaFramework.LowLevel.Unsafe
{
    public enum ReaderError
    {
        None,
        Overflow,
        OutOfRange
    }

    public unsafe struct BBitReader
    {
        public readonly byte* m_Buffer;

        public int m_Index;
        public int m_Length;

        public bool IsInitialized => m_Buffer != null && m_Length > 0;

        public bool IsValid => IsInitialized && m_Index >= 0;

        public int Index => m_Index;
        public int Length => m_Length;

        public BBitReader(in BByteReader reader)
        {
            m_Buffer = reader.m_Head;

            m_Index = 0;
            m_Length = (reader.Length - reader.Index) * 8;
        }

        public BBitReader(NativeList<byte> list)
        {
            m_Buffer = (byte*)list.GetUnsafeReadOnlyPtr();

            m_Index = 0;
            m_Length = list.Length * 8;
        }

        public BBitReader(byte* buffer, int length)
        {
            m_Buffer = buffer;

            m_Index = 0;
            m_Length = length * 8;
        }

        public bool HasEnoughBits(int bits) => m_Length - m_Index >= bits;

        public byte ReadBitWithoutCheck()
        {
            var p = (m_Index >> 3);
            var o = 0x07 - (m_Index & 0x07);
            var val = (m_Buffer[p] >> o) & 0x01;
            m_Index++;
            return (byte)val;
        }

        public ReaderError TryReadBits(uint bits, out uint result)
        {
            result = 0;
            if (!HasEnoughBits((int)bits))
                return ReaderError.OutOfRange;

            for (int i = 0; i < bits; i++)
            {
                result = (result << 1) | ReadBitWithoutCheck();
            }
            return ReaderError.None;
        }

        public ReaderError TryReadBool(out bool result) 
        {
            result = false;
            if (!HasEnoughBits(1))
                return ReaderError.OutOfRange;

            result = ReadBitWithoutCheck() == 1;
            return ReaderError.None;
        }

        public ReaderError TryReadUExpGolomb(out uint result)
        {
            result = 0;

            if (!HasEnoughBits(1))
                return ReaderError.OutOfRange;

            var zeros = 0;
            while (ReadBitWithoutCheck() == 0)
            {
                zeros++;
                if (!HasEnoughBits(zeros + 1))
                    return ReaderError.OutOfRange;

                if (zeros >= 32)
                    return ReaderError.Overflow;
            }

            result = 1u << zeros;
            for (var i = zeros - 1; i >= 0; i--)
                result |= (uint)(ReadBitWithoutCheck() << i);

            result -= 1;

            return ReaderError.None;
        }

        public ReaderError TryReadSExpGolomb(out int result)
        {
            result = 0;
            var error = TryReadUExpGolomb(out var value);
            if (error != ReaderError.None)
                return error;

            value += 1;

            result = value > 1 ? (value & 0x01) == 0 ? (int)(value >> 1) : -(int)(value >> 1) : 0;
            return ReaderError.None;
        }
    }
}
