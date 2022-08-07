using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaFramework.LowLevel.Unsafe
{
    public unsafe static class BigEndian
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16(byte* data) => (ushort)(data[0] << 8 | data[1]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(byte* data) =>
                (uint)data[0] << 24 | (uint)data[1] << 16 | (uint)data[2] << 8 | data[3];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Read2UInt32(byte* data) =>
            (ulong)data[4] << 56 | (ulong)data[5] << 48 | (ulong)data[6] << 40 | (ulong)data[7] << 32 |
            (ulong)data[0] << 24 | (ulong)data[1] << 16 | (ulong)data[2] << 8 | data[3];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64(byte* data) =>
            (ulong)data[0] << 56 | (ulong)data[1] << 48 | (ulong)data[2] << 40 | (ulong)data[3] << 32 |
            (ulong)data[4] << 24 | (ulong)data[5] << 16 | (ulong)data[6] << 8 | data[7];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(byte* data) => (short)(data[0] << 8 | data[1]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(byte* data) =>
            (int)data[0] << 24 | (int)data[1] << 16 | (int)data[2] << 8 | data[3];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(byte* data) =>
            (long)data[0] << 56 | (long)data[1] << 48 | (long)data[2] << 40 | (long)data[3] << 32 |
            (long)data[4] << 24 | (long)data[5] << 16 | (long)data[6] << 8 | data[7];

        public static void WriteUInt16(byte* data, ushort value)
        {
            data[0] = (byte)(value >> 8);
            data[1] = (byte)value;
        }

        public static void WriteUInt24(byte* data, uint value)
        {
            data[0] = (byte)(value >> 16);
            data[1] = (byte)(value >> 8);
            data[2] = (byte)value;
        }

        public static void WriteUInt32(byte* data, uint value)
        {
            data[0] = (byte)(value >> 24);
            data[1] = (byte)(value >> 16);
            data[2] = (byte)(value >> 8);
            data[3] = (byte)value;
        }

        public static void WriteUInt64(byte* data, ulong value)
        {
            data[0] = (byte)(value >> 56);
            data[1] = (byte)(value >> 48);
            data[2] = (byte)(value >> 40);
            data[3] = (byte)(value >> 32);
            data[4] = (byte)(value >> 24);
            data[5] = (byte)(value >> 16);
            data[6] = (byte)(value >> 8);
            data[7] = (byte)value;
        }

        public static void WriteInt16(byte* data, short value) => WriteUInt16(data, (ushort)value);

        public static void WriteInt32(byte* data, int value) => WriteUInt32(data, (uint)value);

        public static void WriteInt64(byte* data, long value) => WriteUInt64(data, (ulong)value);

        public static string ConvertToString(uint data) =>
            new string(new char[4] {
                (char)((data & 0xFF000000) >> 24),
                (char)((data & 0x00FF0000) >> 16),
                (char)((data & 0x0000FF00) >> 8),
                (char)((data & 0x000000FF))
            });

        public static uint ConvertFourCCToUInt32(string data) =>
            (uint)data[0] << 24 | (uint)data[1] << 16 | (uint)data[2] << 8 | (uint)data[3];
    }
}
