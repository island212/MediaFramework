using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace MediaFramework.LowLevel.MP4
{
    // 4.2 ISO/IEC 14496-12:2015(E)
    public struct ISOBox
    {
        public const int ByteNeeded = 8;

        public uint Size;           // 32 bits
        public ISOBoxType Type;     // 32 bits

        public ISOBox(uint size, ISOBoxType type)
        {
            Size = size;
            Type = type;
        }
    }

    // 4.2 ISO/IEC 14496-12:2015(E)
    public struct ISOFullBox
    {
        //public const int ByteNeeded = 12;

        //[StructLayout(LayoutKind.Sequential, Size = 8)]
        //public struct VersionFlags
        //{
        //    public byte Version;        // 8 bits
        //    public uint Flags;          // 24 bits

        //    public VersionFlags(byte version, uint flags)
        //    {
        //        Version = version;
        //        Flags = flags;
        //    }
        //}

        //public uint Size;               // 32 bits
        //public ISOBoxType Type;         // 32 bits
        //public VersionFlags Details;    // 32 bits

        //public ISOFullBox(uint size, ISOBoxType type, VersionFlags details)
        //{
        //    Size = size;
        //    Type = type;
        //    Details = details;
        //}

        //public unsafe static ISOFullBox Parse(byte* buffer) => new
        //(
        //    BigEndian.ReadUInt32(buffer),
        //    (ISOBoxType)BigEndian.ReadUInt32(buffer + 4),
        //    GetDetails(buffer)
        //);

        //public unsafe static VersionFlags GetDetails(byte* buffer) => new
        //(
        //    *(buffer + 8),
        //    // Peek the version and flags then remove the version
        //    BigEndian.ReadUInt32(buffer + 8) & 0x00FFFFFF
        //);

        //public unsafe static byte GetVersion(byte* buffer) => *(buffer + 8);

        //public unsafe static uint GetFlags(byte* buffer) => BigEndian.ReadUInt32(buffer + 8) & 0x00FFFFFF;
    }

    public struct ISODate
    {
        public ulong value;
    }

    // ISO 639-2 Code
    // https://en.wikipedia.org/wiki/List_of_ISO_639-2_codes
    public struct ISOLanguage
    {
        // padding (1 bit) + character (5 bits)[3]
        public ushort value;
    }

    public struct FixedPoint1616Matrix3x3
    {
        public const int ByteNeeded = 36;

        public int3x3 value;
    }

    public struct FixedPoint1616
    {
        public int value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ConvertDouble() => ConvertDouble(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ConvertDouble(int value)
            => value > 0 ? value / 65536.0 : 0;
    }

    public struct UFixedPoint1616
    {
        public uint value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ConvertDouble() => ConvertDouble(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ConvertDouble(uint value)
            => value > 0 ? value / 65536.0 : 0;
    }

    public struct FixedPoint88
    {
        public short value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ConvertDouble() => ConvertDouble(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ConvertDouble(short value)
            => value > 0 ? value / 256.0 : 0;
    }

    public struct SampleGroup
    {
        public uint Count;
        public uint Delta;
    }

    public struct TRAKBox
    {
        public TKHDBox TKHD;

        public static MP4Error Read(ref MP4JobContext context, ref TRAKBox track, in ISOBox box)
        {
            MP4BoxUtility.CheckForBoxType(box.Type, ISOBoxType.TRAK);
            MP4BoxUtility.CheckForValidContext(context);

            var error = context.ValidateBox(box);
            if (error != MP4Error.None)
                return error;

            int rIdx = context.Reader.Index;

            uint i = 0;
            while (i < box.Size)
            {
                var isoBox = context.Reader.ReadISOBox();

                switch (isoBox.Type)
                {
                    case ISOBoxType.TKHD:
                        error = TKHDBox.Read(ref context, ref track.TKHD, isoBox);
                        break;
                    case ISOBoxType.MDIA:
                    case ISOBoxType.TREF:
                    case ISOBoxType.TRGR:
                    case ISOBoxType.EDTS:
                    case ISOBoxType.META:
                        context.Reader.Seek((int)isoBox.Size);
                        break;
                    default:
                        if (context.Policy == Validation.Strict)
                        {
                            return context.LogTrackError(MP4Error.InvalidBoxType,
                                $"The box {isoBox.Type} is either invalid or can't be found inside a TRAK box");
                        }
                        context.Reader.Seek((int)isoBox.Size);
                        break;
                }

                if (error != MP4Error.None)
                    return error;

                if (context.Reader.Index - rIdx != box.Size)
                    return context.LogTrackError(MP4Error.InvalidBoxType, $"The box {isoBox.Type} is either invalid or can't be found inside a TRAK box");

                MP4BoxUtility.CheckIfReaderRead(rIdx, context.Reader, isoBox);

                rIdx = context.Reader.Index;
                i += isoBox.Size;
            }

            return MP4Error.None;
        }      
    }

    public struct TKHDBox
    {
        public const int Version0 = 92;
        public const int Version1 = 104;

        public uint TrackID;
        public ulong Duration;

        public static MP4Error Read(ref MP4JobContext context, ref TKHDBox tkhd, in ISOBox box)
        {
            MP4BoxUtility.CheckForBoxType(box.Type, ISOBoxType.TKHD);
            MP4BoxUtility.CheckForValidContext(context);

            var error = context.ValidateFullBox(box, Version0, Version1);
            if (error != MP4Error.None)
                return error;

            var version = context.Reader.ReadUInt8();
            context.Reader.Seek(3); // flags

            switch (version)
            {
                case 0:
                    if (box.Size != Version0)
                        return context.LogTrackError(MP4Error.InvalidBoxSize, $"The TKHD box size for version 0 should be {Version0} and not {box.Size}");

                    context.Reader.Seek(4); // creation_time
                    context.Reader.Seek(4); // modification_time
                    tkhd.TrackID = context.Reader.ReadUInt32();
                    context.Reader.Seek(4); // reserved
                    tkhd.Duration = context.Reader.ReadUInt32();
                    break;
                case 1:
                    if (box.Size != Version1)
                        return context.LogTrackError(MP4Error.InvalidBoxSize, $"The TKHD box size for version 1 should be {Version1} and not {box.Size}");

                    context.Reader.Seek(8); // creation_time
                    context.Reader.Seek(8); // modification_time
                    tkhd.TrackID = context.Reader.ReadUInt32();
                    context.Reader.Seek(4); // reserved
                    tkhd.Duration = context.Reader.ReadUInt64();
                    break;
                default:
                    return context.LogTrackError(MP4Error.InvalidBoxSize, $"The TKHD box has an invalid version of {version}");
            }

            context.Reader.Seek(8); // reserved
            context.Reader.Seek(2); // layer 
            context.Reader.Seek(2); // alternate_group  
            context.Reader.Seek(2); // volume 
            context.Reader.Seek(2); // reserved
            context.Reader.Seek(4 * 9); // matrix
            context.Reader.Seek(4); // width
            context.Reader.Seek(4); // height

            return MP4Error.None;
        }
    }

    public struct MP4JobTrack
    {


        public uint TimeScale;
    }

    public static class MP4Math
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvertFixedPoint16(int value) => value >> 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertFixedPoint16(uint value) => value >> 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvertFixedPoint8(int value) => value >> 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertFixedPoint8(uint value) => value >> 8;
    }

    public static class MP4BoxUtility
    {
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckForBoxType(ISOBoxType value, ISOBoxType compare)
        {
            if (value != compare)
                throw new ArgumentException($"The box type was {value} but expected {compare} box");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckForValidContext(in MP4JobContext context)
        {

        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckIfReaderRead(int index, in BByteReader reader, in ISOBox box)
        {
            if(index >= reader.Index)
                throw new ArgumentException($"Infinite loop detected. The reader didn't read when parsing {box.Type} box");
        }
    }

    // Useful to convert FourCC to HEX
    // https://www.branah.com/ascii-converter

    /// <summary>
    /// FourCC boxes. The value is the FourCC in decimal
    /// </summary>
    public enum ISOBoxType : uint
    {
        BXML = 0x62786D6C,
        CO64 = 0x636f3634,
        CPRT = 0x63707274,
        CSLG = 0x63736c67,
        CTTS = 0x63747473,
        DINF = 0x64696e66,
        DREF = 0x64726566,
        EDTS = 0x65647473,
        ELNG = 0x656c6e67,
        ELST = 0x656c7374,
        FECR = 0x66656372,
        FIIN = 0x6669696E,
        FIRE = 0x66697265,
        FPAR = 0x66706172,
        FREE = 0x66726565,
        FRMA = 0x66726D61,
        FTYP = 0x66747970,
        GITN = 0x6769746E,
        HDLR = 0x68646c72,
        HMHD = 0x686d6864,
        IDAT = 0x69646174,
        IINF = 0x69696E66,
        ILOC = 0x696C6F63,
        IPRO = 0x6970726F,
        IREF = 0x69726566,
        LEVA = 0x6c657661,
        MDAT = 0x6d646174,
        MDHD = 0x6d646864,
        MDIA = 0x6d646961,
        MECO = 0x6D65636F,
        MEHD = 0x6d656864,
        MERE = 0x6D657265,
        META = 0x6d657461,
        MFHD = 0x6D666864,
        MFRA = 0x6D667261,
        MFRO = 0x6D66726F,
        MINF = 0x6d696e66,
        MOOF = 0x6D6F6F66,
        MOOV = 0x6d6f6f76,
        MVEX = 0x6d766578,
        MVHD = 0x6d766864,
        NMHD = 0x6e6d6864,
        PADB = 0x70616462,
        PAEN = 0x7061656E,
        PDIN = 0x7064696e,
        PITM = 0x7069746D,
        PRFT = 0x70726674,
        SAIO = 0x7361696f,
        SAIZ = 0x7361697a,
        SBGP = 0x73626770,
        SCHI = 0x73636869,
        SCHM = 0x7363686D,
        SDTP = 0x73647470,
        SEGR = 0x73656772,
        SGPD = 0x73677064,
        SIDX = 0x73696478,
        SINF = 0x73696E66,
        SKIP = 0x736b6970,
        SMHD = 0x736d6864,
        SSIX = 0x73736978,
        STBL = 0x7374626c,
        STCO = 0x7374636f,
        STDP = 0x73746470,
        STHD = 0x73746864,
        STRD = 0x73747264,
        STRI = 0x73747269,
        STRK = 0x7374726B,
        STSC = 0x73747363,
        STSD = 0x73747364,
        STSH = 0x73747368,
        STSS = 0x73747373,
        STSZ = 0x7374737a,
        STTS = 0x73747473,
        STYP = 0x73747970,
        STZ2 = 0x73747a32,
        SUBS = 0x73756273,
        TFDT = 0x74666474,
        TFHD = 0x74666864,
        TFRA = 0x74667261,
        TKHD = 0x746b6864,
        TRAF = 0x74726166,
        TRAK = 0x7472616b,
        TREF = 0x74726566,
        TREX = 0x74726578,
        TRGR = 0x74726772,
        TRUN = 0x7472756E,
        TSEL = 0x7473656C,
        UDTA = 0x75647461,
        UUID = 0x75756964,
        VMHD = 0x766d6864,
        WIDE = 0x77696465,
        XML_ = 0x786D6C20,

        // 6.2.1 ISO/IEC 14496-12:2015
        // Should not be use anymore but was in previous implementation
        CHAP = 0x63686170,
        CLIP = 0x636c6970,
        CRGN = 0x6372676e,
        CTAB = 0x63746162,
        IMAP = 0x696d6170,
        KMAT = 0x6b6d6174,
        LOAD = 0x6c6f6164,
        MATT = 0x6d617474,
        PNOT = 0x706e6f74,
        SCPT = 0x73637074,
        SYNC = 0x73796e63,
        TMCD = 0x746d6364,
    }

    /// <summary>
    /// FourCC brands. The value is the FourCC in decimal
    /// </summary>
    public enum ISOBrand : uint
    {
        AVC1 = 0x61766331,
        ISO2 = 0x69736f32,
        ISO3 = 0x69736f33,
        ISO4 = 0x69736f34,
        ISO5 = 0x69736f35,
        ISO6 = 0x69736f36,
        ISO7 = 0x69736f37,
        ISO8 = 0x69736f38,
        ISO9 = 0x69736f39,
        ISOM = 0x69736f6d,
        MP41 = 0x6d703431,
        MP42 = 0x6d703432,
        MP71 = 0x6d703731,
    }

    public enum ISOHandler
    {
        None = 0,
        VIDE = 0x76696465,
        SOUN = 0x736f756e
    }
}
