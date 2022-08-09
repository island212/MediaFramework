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

    public struct SampleGroup
    {
        public uint Count;
        public uint Delta;
    }

    public static class ISOBMFF
    {
        public static MP4Error Read(ref MP4JobContext context, ref BByteReader reader, ISOBox box)
        {
            if (context.BoxDepth > 7)
                return context.LogError(MP4Error.IllegalBoxDepth, $"The box chain is nested too depth");

            var error = MP4Error.None;

            context.BoxDepth++;

            int lastBytePosition = (int)box.Size + reader.Index - ISOBox.ByteNeeded;
            if(lastBytePosition > reader.Length)
                return context.LogError(MP4Error.IndexOutOfReaderRange,
                    $"The {box.Type} box size is {box.Size} and is bigger than the amount of remaining bytes ({reader.Remains}) in the reader");

            while (reader.Index + ISOBox.ByteNeeded < lastBytePosition)
            {
                var isoBox = reader.ReadISOBox();

                switch (isoBox.Type)
                {
                    case ISOBoxType.MVHD:
                        error = MVHDBox.Read(ref context, ref reader, isoBox);
                        break;
                    case ISOBoxType.TRAK:
                        error = TRAKBox.Read(ref context, ref reader, isoBox);
                        break;
                    case ISOBoxType.TKHD:
                        error = TKHDBox.Read(ref context, ref reader, isoBox);
                        break;
                    case ISOBoxType.STBL:
                    case ISOBoxType.MINF:
                    case ISOBoxType.MDIA:
                        error = Read(ref context, ref reader, isoBox);
                        break;
                    default:
                        reader.Seek((int)isoBox.Size - ISOBox.ByteNeeded);
                        break;
                }

                if (error != MP4Error.None)
                    return error;
            }

            context.BoxDepth--;

            return error;
        }
    }

    public struct MVHDBox
    {
        public const int Version0 = 108;
        public const int Version1 = 120;

        public ulong Duration;
        public uint TimeScale;
        public uint NextTrackID;

        public static MP4Error Read(ref MP4JobContext context, ref BByteReader reader, in ISOBox box)
        {
            if (reader.Remains >= Version0)
                return context.LogError(MP4Error.IndexOutOfReaderRange, 
                    $"The {box.Type} box size is {box.Size} and is bigger than the amount of remaining bytes ({reader.Remains}) in the reader");

            var version = reader.ReadUInt8();
            reader.Seek(3); // flags

            switch (version)
            {
                case 0:
                    if (box.Size != Version0)
                        return context.LogError(MP4Error.InvalidBoxSize, $"The MVHD box size for version 0 should be {Version0} and not {box.Size}");

                    reader.Seek(4); // creation_time
                    reader.Seek(4); // modification_time
                    context.MVHD.TimeScale = reader.ReadUInt32();
                    context.MVHD.Duration = reader.ReadUInt32();
                    break;
                case 1:
                    if (box.Size != Version1)
                        return context.LogError(MP4Error.InvalidBoxSize, $"The MVHD box size for version 1 should be {Version1} and not {box.Size}");

                    reader.Seek(8); // creation_time
                    reader.Seek(8); // modification_time
                    context.MVHD.TimeScale = reader.ReadUInt32();
                    context.MVHD.Duration = reader.ReadUInt64();
                    break;
                default:
                    return context.LogError(MP4Error.InvalidBoxVersion, $"The MVHD box has an invalid version of {version}");
            }

            reader.Seek(4); // rate
            reader.Seek(2); // volume  
            reader.Seek(2); // reserved   
            reader.Seek(4 * 2); // reserved  
            reader.Seek(4 * 9); // matrix
            reader.Seek(4 * 6); // pre_defined 
            context.MVHD.NextTrackID = reader.ReadUInt32();

            return MP4Error.None;
        }
    }

    public struct TRAKBox
    {
        public TKHDBox TKHD;

        public static MP4Error Read(ref MP4JobContext context, ref BByteReader reader, in ISOBox box)
        {
            context.Tracks.Add(new TRAKBox());

            var error = ISOBMFF.Read(ref context, ref reader, box);

            ref var track = ref context.CurrentTrack;

            return error;
        }      
    }

    public struct TKHDBox
    {
        public const int Version0 = 92;
        public const int Version1 = 104;

        public uint TrackID;

        public static MP4Error Read(ref MP4JobContext context, ref BByteReader reader, in ISOBox box)
        {
            if (reader.Remains >= Version0)
                return context.LogError(MP4Error.IndexOutOfReaderRange,
                    $"The {box.Type} box size is {box.Size} and is bigger than the amount of remaining bytes ({reader.Remains}) in the reader");

            ref var track = ref context.CurrentTrack;

            var version = reader.ReadUInt8();
            reader.Seek(3); // flags

            switch (version)
            {
                case 0:
                    if (box.Size != Version0)
                        return context.LogError(MP4Error.InvalidBoxSize, $"The TKHD box size for version 0 should be {Version0} and not {box.Size}");

                    reader.Seek(4); // creation_time
                    reader.Seek(4); // modification_time
                    track.TKHD.TrackID = reader.ReadUInt32();
                    reader.Seek(4); // reserved
                    reader.Seek(4); // duration
                    break;
                case 1:
                    if (box.Size != Version1)
                        return context.LogError(MP4Error.InvalidBoxSize, $"The TKHD box size for version 1 should be {Version1} and not {box.Size}");

                    reader.Seek(8); // creation_time
                    reader.Seek(8); // modification_time
                    track.TKHD.TrackID = reader.ReadUInt32();
                    reader.Seek(4); // reserved
                    reader.Seek(8); // duration
                    break;
                default:
                    return context.LogError(MP4Error.InvalidBoxVersion, $"The TKHD box has an invalid version of {version}");
            }

            reader.Seek(8); // reserved
            reader.Seek(2); // layer 
            reader.Seek(2); // alternate_group  
            reader.Seek(2); // volume 
            reader.Seek(2); // reserved
            reader.Seek(4 * 9); // matrix
            reader.Seek(4); // width
            reader.Seek(4); // height

            return MP4Error.None;
        }
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
        public static void CheckIfReaderRead(int index, in BByteReader reader, in ISOBox box)
        {
            if(index >= reader.Index)
                throw new InvalidOperationException($"Infinite loop detected. The reader didn't progress when reading {box.Type} box");
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
