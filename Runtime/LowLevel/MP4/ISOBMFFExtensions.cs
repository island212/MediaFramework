using MediaFramework.LowLevel.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFramework.LowLevel.MP4
{
    public static class ISOBMFFExtensions
    {
        public unsafe static ISOBox PeekISOBox(in this BByteReader reader, int offset) => new ISOBox
        {
            size =  BigEndian.ReadUInt32(reader.m_Head + offset),
            type = (ISOBoxType)BigEndian.ReadUInt32(reader.m_Head + offset + 4),
        };

        public unsafe static ISOBox ReadISOBox(ref this BByteReader reader) => new ISOBox
        {
            size = reader.ReadUInt32(),
            type = (ISOBoxType)reader.ReadUInt32(),
        };
    }
}
