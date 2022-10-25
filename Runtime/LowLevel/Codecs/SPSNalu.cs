using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using MediaFramework.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.TestTools;

namespace MediaFramework.LowLevel.Codecs
{
    public enum ChromaSubsampling
    {
        YUV400 = 0,
        YUV420 = 1,
        YUV422 = 2,
        YUV444 = 3
    }

    public enum ColorPrimaries
    {
        RESERVED0 = 0,
        BT709 = 1,                  ///< also ITU-R BT1361 / IEC 61966-2-4 / SMPTE RP 177 Annex B
        UNSPECIFIED = 2,
        RESERVED = 3,
        BT470M = 4,                 ///< also FCC Title 47 Code of Federal Regulations 73.682 (a)(20)                                    
        BT470BG = 5,                ///< also ITU-R BT601-6 625 / ITU-R BT1358 625 / ITU-R BT1700 625 PAL & SECAM
        SMPTE170M = 6,              ///< also ITU-R BT601-6 525 / ITU-R BT1358 525 / ITU-R BT1700 NTSC
        SMPTE240M = 7,              ///< identical to above, also called "SMPTE C" even though it uses D65
        FILM = 8,                   ///< colour filters using Illuminant C
        BT2020 = 9,                 ///< ITU-R BT2020
        SMPTE428 = 10,              ///< SMPTE ST 428-1 (CIE 1931 XYZ)
        SMPTEST428_1 = SMPTE428,
        SMPTE431 = 11,              ///< SMPTE ST 431-2 (2011) / DCI P3
        SMPTE432 = 12,              ///< SMPTE ST 432-1 (2010) / P3 D65 / Display P3
        EBU3213 = 22,               ///< EBU Tech. 3213-E (nothing there) / one of JEDEC P22 group phosphors
        JEDEC_P22 = EBU3213,
    }

    public enum ColorTransferCharacteristic
    {
        RESERVED0 = 0,
        BT709 = 1,                  ///< also ITU-R BT1361
        UNSPECIFIED = 2,
        RESERVED = 3,
        GAMMA22 = 4,                ///< also ITU-R BT470M / ITU-R BT1700 625 PAL & SECAM
        GAMMA28 = 5,                ///< also ITU-R BT470BG
        SMPTE170M = 6,              ///< also ITU-R BT601-6 525 or 625 / ITU-R BT1358 525 or 625 / ITU-R BT1700 NTSC
        SMPTE240M = 7,
        LINEAR = 8,                 ///< "Linear transfer characteristics"
        LOG = 9,                    ///< "Logarithmic transfer characteristic (100:1 range)"
        LOG_SQRT = 10,              ///< "Logarithmic transfer characteristic (100 * Sqrt(10) : 1 range)"
        IEC61966_2_4 = 11,          ///< IEC 61966-2-4
        BT1361_ECG = 12,            ///< ITU-R BT1361 Extended Colour Gamut
        IEC61966_2_1 = 13,          ///< IEC 61966-2-1 (sRGB or sYCC)
        BT2020_10 = 14,             ///< ITU-R BT2020 for 10-bit system
        BT2020_12 = 15,             ///< ITU-R BT2020 for 12-bit system
        SMPTE2084 = 16,             ///< SMPTE ST 2084 for 10-, 12-, 14- and 16-bit systems
        SMPTEST2084 = SMPTE2084,
        SMPTE428 = 17,              ///< SMPTE ST 428-1
        SMPTEST428_1 = SMPTE428,
        ARIB_STD_B67 = 18,          ///< ARIB STD-B67, known as "Hybrid log-gamma"
    }

    public enum ColorMatrix
    {
        RGB = 0,                    ///< order of coefficients is actually GBR, also IEC 61966-2-1 (sRGB), YZX and ST 428-1
        BT709 = 1,                  ///< also ITU-R BT1361 / IEC 61966-2-4 xvYCC709 / derived in SMPTE RP 177 Annex B
        UNSPECIFIED = 2,            
        RESERVED = 3,               ///< reserved for future use by ITU-T and ISO/IEC just like 15-255 are
        FCC = 4,                    ///< FCC Title 47 Code of Federal Regulations 73.682 (a)(20)
        BT470BG = 5,                ///< also ITU-R BT601-6 625 / ITU-R BT1358 625 / ITU-R BT1700 625 PAL & SECAM / IEC 61966-2-4 xvYCC601
        SMPTE170M = 6,              ///< also ITU-R BT601-6 525 / ITU-R BT1358 525 / ITU-R BT1700 NTSC / functionally identical to above
        SMPTE240M = 7,              ///< derived from 170M primaries and D65 white point, 170M is derived from BT470 System M's primaries
        YCGCO = 8,                  ///< used by Dirac / VC-2 and H.264 FRext, see ITU-T SG16
        YCOCG = YCGCO,
        BT2020_NCL = 9,             ///< ITU-R BT2020 non-constant luminance system
        BT2020_CL = 10,             ///< ITU-R BT2020 constant luminance system
        SMPTE2085 = 11,             ///< SMPTE 2085, Y'D'zD'x
        CHROMA_DERIVED_NCL = 12,    ///< Chromaticity-derived non-constant luminance system
        CHROMA_DERIVED_CL = 13,     ///< Chromaticity-derived constant luminance system
        ICTCP = 14,                 ///< ITU-R BT.2100-0, ICtCp
    }

    public enum SPSError
    {
        None = ReaderError.None,
        ReaderOverflow = ReaderError.Overflow,
        ReaderOutOfRange = ReaderError.OutOfRange,
        InvalidLength = 1000,
        ForbiddenZeroBit,
        InvalidRefID,
        InvalidUnitType,
        InvalidSeqParamaterSetId,
        InvalidChromaFormat,
        InvalidBitDepthLuma,
        InvalidBitDepthChroma,
        ConflictingBitDepth,
        InvalidScalingMatrixDeltaScale,
        InvalidMaxFrameNum,
        InvalidPicOrderCntType,
        InvalidMaxPictureOrderCntLsb,
        InvalidNumRefFramesInCycle,
        InvalidMaxNumRefFrames,
        InvalidMbWidth,
        InvalidMbHeight,
        InvalidCrop,
        InvalidAspectIndicator,
        InvalidAspectExtendedValue,
        InvalidVideoFormat,
        InvalidChromaLocTypeField,
        InvalidTimeInfo,
    }

    public unsafe struct ScalingMatrix
    {
        public static readonly byte[] DefaultIntra4x4 = new byte[] 
        { 
            6, 13, 13, 20, 20, 20, 28, 28, 28, 28, 32, 32, 32, 37, 37, 42 
        };

        public static readonly byte[] DefaultInter4x4 = new byte[] 
        { 
            10, 14, 14, 20, 20, 20, 24, 24, 24, 24, 27, 27, 27, 30, 30, 34 
        };

        public static readonly byte[] DefaultIntra8x8 = new byte[]
        {
            06, 10, 10, 13, 11, 13, 16, 16, 16, 16, 18, 18, 18, 18, 18, 23,
            23, 23, 23, 23, 23, 25, 25, 25, 25, 25, 25, 25, 27, 27, 27, 27,
            27, 27, 27, 27, 29, 29, 29, 29, 29, 29, 29, 31, 31, 31, 31, 31,
            31, 33, 33, 33, 33, 33, 36, 36, 36, 36, 38, 38, 38, 40, 40, 42,
        };

        public static readonly byte[] DefaultInter8x8 = new byte[]
        {
            09, 13, 13, 15, 13, 15, 17, 17, 17, 17, 19, 19, 19, 19, 19, 21,
            21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 24, 24, 24, 24,
            24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 27, 27, 27, 27, 27,
            27, 28, 28, 28, 28, 28, 30, 30, 30, 30, 32, 32, 32, 33, 33, 35,
        };

        public bool IsCreated => m_Buffer != null;

        public byte* m_Buffer;

        public byte* mat00 => m_Buffer;
        public byte* mat01 => m_Buffer + 16;
        public byte* mat02 => m_Buffer + 32;
        public byte* mat03 => m_Buffer + 48;
        public byte* mat04 => m_Buffer + 64;
        public byte* mat05 => m_Buffer + 80;
        public byte* mat06 => m_Buffer + 96;
        public byte* mat07 => m_Buffer + 160;
        public byte* mat08 => m_Buffer + 224;
        public byte* mat09 => m_Buffer + 288;
        public byte* mat10 => m_Buffer + 352;
        public byte* mat11 => m_Buffer + 416;

        public SPSError Parse(ref BBitReader reader, bool isYUV444, Allocator allocator)
        {
            m_Buffer = (byte*)UnsafeUtility.Malloc(isYUV444 ? 480 : 224, 4, allocator);

            fixed (byte* default4Intra = DefaultIntra4x4)
            fixed (byte* default4Inter = DefaultInter4x4)
            fixed (byte* default8Intra = DefaultIntra8x8)
            fixed (byte* default8Inter = DefaultInter8x8)
            {
                SPSError error;

                error = ParseScalingMatrices(ref reader, mat00, 16, default4Intra, default4Intra);  // Intra Y
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat01, 16, default4Intra, mat00);          // Intra Cr
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat02, 16, default4Intra, mat01);          // Intra Cb
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat03, 16, default4Inter, default4Inter);  // Inter Y
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat04, 16, default4Intra, mat03);          // Inter Cr
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat05, 16, default4Intra, mat04);          // Inter Cb
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat06, 64, default8Intra, default8Intra);  // Intra Y
                if (error != SPSError.None) return error;
                error = ParseScalingMatrices(ref reader, mat07, 64, default8Inter, default8Inter);  // Inter Y
                if (error != SPSError.None) return error;

                if (isYUV444)
                {
                    error = ParseScalingMatrices(ref reader, mat08, 64, default8Intra, mat06);      // Intra Cr
                    if (error != SPSError.None) return error;
                    error = ParseScalingMatrices(ref reader, mat09, 64, default8Inter, mat07);      // Inter Cr
                    if (error != SPSError.None) return error;
                    error = ParseScalingMatrices(ref reader, mat10, 64, default8Intra, mat08);      // Intra Cb
                    if (error != SPSError.None) return error;
                    error = ParseScalingMatrices(ref reader, mat11, 64, default8Inter, mat09);      // Inter Cb
                    if (error != SPSError.None) return error;
                }

                return SPSError.None;
            }
        }

        private unsafe static SPSError ParseScalingMatrices(ref BBitReader reader, byte* scalingList, int size, byte* defaultMatrix, byte* fallback)
        {
            var error = reader.TryReadBool(out var seq_scaling_list_present_flag);
            if (error != ReaderError.None)
                return (SPSError)error;

            if (seq_scaling_list_present_flag)
            {
                var lastScale = 8;
                var nextScale = 8;

                int deltaScale;
                if ((error = reader.TryReadSExpGolomb(out deltaScale)) != ReaderError.None)
                    return (SPSError)error;

                if (deltaScale < -128 || deltaScale > 127)
                    return SPSError.InvalidScalingMatrixDeltaScale;

                nextScale = (lastScale + deltaScale) & 0xFF;

                if (nextScale == 0)
                {
                    UnsafeUtility.MemCpy(scalingList, defaultMatrix, size);
                }

                lastScale = nextScale;
                scalingList[0] = (byte)lastScale;

                for (int j = 1; j < size; j++)
                {
                    if (nextScale != 0)
                    {
                        if ((error = reader.TryReadSExpGolomb(out deltaScale)) != ReaderError.None)
                            return (SPSError)error;

                        if (deltaScale < -128 || deltaScale > 127)
                            return SPSError.InvalidScalingMatrixDeltaScale;

                        nextScale = (lastScale + deltaScale) & 0xFF;
                    }

                    lastScale = nextScale == 0 ? lastScale : nextScale;
                    scalingList[j] = (byte)lastScale;
                }
            }
            else
            {
                UnsafeUtility.MemCpy(scalingList, fallback, size);
            }

            return SPSError.None;
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct SPSProfile
    {
        public H264Profile FullProfile => H264Utility.GetProfile(Type, Constraints);

        public uint ProfileLevelId => (uint)Type << 16 | (uint)Constraints << 8 | Level;

        public byte Type;
        public byte Constraints;
        public byte Level;

        [ExcludeFromCoverage]
        public override string ToString() => FullProfile switch
        {
            H264Profile.Baseline => $"Baseline@{(double)Level / 10:0.0}",
            H264Profile.ConstrainedBaseline => $"Baseline(c)@{(double)Level / 10:0.0}",
            H264Profile.Extended => $"Extended@{(double)Level / 10:0.0}",
            H264Profile.Main => $"Main@{(double)Level / 10:0.0}",
            H264Profile.High => $"High@{(double)Level / 10:0.0}",
            H264Profile.ProgressiveHigh => $"High(p)@{(double)Level / 10:0.0}",
            H264Profile.ConstrainedHigh => $"High(c)@{(double)Level / 10:0.0}",
            H264Profile.High10 => $"High10@{(double)Level / 10:0.0}",
            H264Profile.High10Intra => $"High10(i)@{(double)Level / 10:0.0}",
            H264Profile.High422 => $"High422@{(double)Level / 10:0.0}",
            H264Profile.High422Intra => $"High422(i)@{(double)Level / 10:0.0}",
            H264Profile.High444Predictive => $"High444(p)@{(double)Level / 10:0.0}",
            H264Profile.High444Intra => $"High444(i)@{(double)Level / 10:0.0}",
            H264Profile.CAVLC444Intra => $"CAVLC444@{(double)Level / 10:0.0}",
            H264Profile.ScalableBaseline => $"Baseline(s)@{(double)Level / 10:0.0}",
            H264Profile.ScalableConstrainedBaseline => $"Baseline(sc)@{(double)Level / 10:0.0}",
            H264Profile.ScalableHigh => $"High(s)@{(double)Level / 10:0.0}",
            H264Profile.ScalableConstrainedHigh => $"High(sc)@{(double)Level / 10:0.0}",
            H264Profile.ScalableHighIntra => $"High(si)@{(double)Level / 10:0.0}",
            H264Profile.StereoHighProfile => $"StereoHighProfile@{(double)Level / 10:0.0}",
            H264Profile.MultiviewHighProfile => $"MultiviewHighProfile@{(double)Level / 10:0.0}",
            H264Profile.MFCHigh => $"MFCHigh@{(double)Level / 10:0.0}",
            H264Profile.MFCDepthHigh => $"MFCDepthHigh@{(double)Level / 10:0.0}",
            H264Profile.MultiviewDepthHigh => $"MultiviewDepthHigh@{(double)Level / 10:0.0}",
            H264Profile.EnhancedMultiviewDepthHigh => $"EnhancedMultiviewDepthHigh@{(double)Level / 10:0.0}",
            _ => $"{FullProfile}@{(double)Level / 10:0.0}",
        };
    }

    public struct ChromaSampleLocType
    {
        public int TopField;
        public int BottomField;
    }

    /// <summary>
    /// Sequence Parameter Set ITU-T H.264 08/2021 7.3.2.1.1
    /// https://www.itu.int/ITU-T/recommendations/rec.aspx?rec=14659&lang=en
    /// </summary>
    public unsafe struct SPSNalu : IDisposable
    {
        public Allocator Allocator;

        public BitField32 Flags;

        public uint ID;
        public SPSProfile Profile;
        public ChromaSubsampling ChromaFormat;
        public uint BitDepth;
        public ScalingMatrix ScalingMatrix;

        public ColorPrimaries ColourPrimaries;
        public ColorTransferCharacteristic TransferCharacteristics;
        public ColorMatrix MatrixCoefficients;

        public uint MbWidth, MbHeigth;
        public uint CropLeft, CropRight;
        public uint CropTop, CropBottom;
        public IntRational SAR;

        public int MaxFrameNum;
        public int MaxNumRefFrames;

        public uint NumUnitsInTick;
        public uint Timescale;
        public ChromaSampleLocType ChromaLoc;

        public int POCType;
        public int MaxPOCLsb;
        public int OffsetForNonRefPic;
        public int OffsetForTopToBottomField;
        public int NumRefFramesInCycle;
        public int* OffsetRefFrames;

        public uint Width => PictureWidth - CropLeft - CropRight;

        public uint Height => PictureHeight - CropTop - CropBottom;

        public uint PictureWidth => MbWidth << 4;

        public uint PictureHeight => MbHeigth << 4;

        public bool SeparateColourPlane
        {
            get => Flags.IsSet(0);
            set => Flags.SetBits(0, value);
        }

        public bool TransformBypass
        {
            get => Flags.IsSet(1);
            set => Flags.SetBits(1, value);
        }

        public bool DeltaPOCAlwaysZero
        {
            get => Flags.IsSet(2);
            set => Flags.SetBits(2, value);
        }

        public bool GapsInFrameNumValueAllowed
        {
            get => Flags.IsSet(3);
            set => Flags.SetBits(3, value);
        }

        public bool FrameMbsOnly
        {
            get => Flags.IsSet(4);
            set => Flags.SetBits(4, value);
        }

        public bool MbAdaptiveFrameField
        {
            get => Flags.IsSet(5);
            set => Flags.SetBits(5, value);
        }

        public bool Direct8x8Inference
        {
            get => Flags.IsSet(6);
            set => Flags.SetBits(6, value);
        }

        public bool VideoFullRange
        {
            get => Flags.IsSet(7);
            set => Flags.SetBits(7, value);
        }

        public bool FixedFrameRate
        {
            get => Flags.IsSet(8);
            set => Flags.SetBits(8, value);
        }

        public int ArrayType => !SeparateColourPlane ? (int)ChromaFormat : 0;

        public uint RawMbBits => 256u * BitDepth + 2u * MbWidthC * MbHeightC * BitDepth;

        public uint MbWidthC => ChromaFormat switch
        {
            ChromaSubsampling.YUV420 => 8u,
            ChromaSubsampling.YUV422 => 8u,
            ChromaSubsampling.YUV444 => 16u,
            _ => 16u,
        };

        public uint MbHeightC => ChromaFormat switch
        {
            ChromaSubsampling.YUV420 => 8u,
            ChromaSubsampling.YUV422 => 16u,
            ChromaSubsampling.YUV444 => 16u,
            _ => 16u,
        };

        public uint SubWidthC => ChromaFormat switch
        {
            ChromaSubsampling.YUV420 => 2u,
            ChromaSubsampling.YUV422 => 2u,
            ChromaSubsampling.YUV444 => !SeparateColourPlane ? 1u : 0u,
            _ => 0u,
        };

        public uint SubHeightC => ChromaFormat switch
        {
            ChromaSubsampling.YUV420 => 2u,
            ChromaSubsampling.YUV422 => 1u,
            ChromaSubsampling.YUV444 => !SeparateColourPlane ? 1u : 0u,
            _ => 0u,
        };

        public SPSError Parse(BByteReader reader, Allocator allocator)
        {
            Allocator = allocator;

            // Default value
            ColourPrimaries = ColorPrimaries.UNSPECIFIED;
            TransferCharacteristics = ColorTransferCharacteristic.UNSPECIFIED;
            MatrixCoefficients = ColorMatrix.UNSPECIFIED;

            ChromaFormat = ChromaSubsampling.YUV420;
            BitDepth = 8;

            if (reader.Length < 4)
                return SPSError.InvalidLength;

            var controlByte = reader.ReadUInt8();

            // forbidden_zero_bit
            if ((controlByte & 0x80) == 0x80)
                return SPSError.ForbiddenZeroBit;

            // nal_ref_id
            if ((controlByte & 0x60) == 0)
                return SPSError.InvalidRefID;

            // nal_unit_type
            if ((controlByte & 0x1F) != 7)
                return SPSError.InvalidUnitType;

            Profile.Type = reader.ReadUInt8();          // profile_idc
            Profile.Constraints = reader.ReadUInt8();   // profile_iop
            Profile.Level = reader.ReadUInt8();         // level_idc

            var spsData = stackalloc byte[reader.Remains];
            int length = CopyAndSanitizeSPSData(spsData, (byte*)reader.GetUnsafePtr() + reader.Index, reader.Remains);
            var spsReader = new BBitReader(spsData, length);

            var rError = ReaderError.None;

            uint seq_parameter_set_id;
            if ((rError = spsReader.TryReadUExpGolomb(out seq_parameter_set_id)) != ReaderError.None)
                return (SPSError)rError;

            if (seq_parameter_set_id > 31)
                return SPSError.InvalidSeqParamaterSetId;

            ID = seq_parameter_set_id;

            if (H264Utility.HasChroma(Profile.Type))
            {
                uint chroma_format;
                if ((rError = spsReader.TryReadUExpGolomb(out chroma_format)) != ReaderError.None)
                    return (SPSError)rError;

                if (chroma_format > 3)
                    return SPSError.InvalidChromaFormat;

                ChromaFormat = (ChromaSubsampling)chroma_format;
                if (ChromaFormat == ChromaSubsampling.YUV444)
                {
                    if ((rError = spsReader.TryReadBool(out var separateColourPlane)) != ReaderError.None)
                        return (SPSError)rError;

                    SeparateColourPlane = separateColourPlane;
                }

                uint bit_depth_luma_minus8;
                if ((rError = spsReader.TryReadUExpGolomb(out bit_depth_luma_minus8)) != ReaderError.None)
                    return (SPSError)rError;

                if (bit_depth_luma_minus8 > 6)
                    return SPSError.InvalidBitDepthLuma;

                BitDepth = (byte)(bit_depth_luma_minus8 + 8);

                uint bit_depth_chroma_minus8;
                if ((rError = spsReader.TryReadUExpGolomb(out bit_depth_chroma_minus8)) != ReaderError.None)
                    return (SPSError)rError;

                if (bit_depth_chroma_minus8 > 6)
                    return SPSError.InvalidBitDepthChroma;

                if (BitDepth != bit_depth_chroma_minus8 + 8)
                    return SPSError.ConflictingBitDepth;

                if ((rError = spsReader.TryReadBool(out var transformBypass)) != ReaderError.None)
                    return (SPSError)rError;

                TransformBypass = transformBypass;

                if ((rError = spsReader.TryReadBool(out var matrixPresent)) != ReaderError.None)
                    return (SPSError)rError;

                if (matrixPresent)
                {
                    var matError = ScalingMatrix.Parse(ref spsReader, ChromaFormat == ChromaSubsampling.YUV444, allocator);
                    if (matError != SPSError.None)
                        return matError;
                }
            }

            if ((rError = spsReader.TryReadUExpGolomb(out var log2_max_frame_num_minus4)) != ReaderError.None)
                return (SPSError)rError;

            if (log2_max_frame_num_minus4 > 12)
                return SPSError.InvalidMaxFrameNum;

            MaxFrameNum = 1 << (int)(log2_max_frame_num_minus4 + 4);

            if ((rError = spsReader.TryReadUExpGolomb(out var pic_order_cnt_type)) != ReaderError.None)
                return (SPSError)rError;

            if (pic_order_cnt_type > 2)
                return SPSError.InvalidPicOrderCntType;

            POCType = (int)pic_order_cnt_type;

            switch (POCType)
            {
                case 0:
                    if ((rError = spsReader.TryReadUExpGolomb(out var log2_max_poc_lsb_minus4)) != ReaderError.None)
                        return (SPSError)rError;

                    if (log2_max_poc_lsb_minus4 > 12)
                        return SPSError.InvalidMaxPictureOrderCntLsb;

                    MaxPOCLsb = 1 << (int)(log2_max_poc_lsb_minus4 + 4);
                    break;
                case 1:
                    if ((rError = spsReader.TryReadBool(out var delta_always_zero)) != ReaderError.None)
                        return (SPSError)rError;

                    DeltaPOCAlwaysZero = delta_always_zero;

                    if ((rError = spsReader.TryReadSExpGolomb(out var offset_for_non_ref_pic)) != ReaderError.None)
                        return (SPSError)rError;

                    OffsetForNonRefPic = offset_for_non_ref_pic;

                    if ((rError = spsReader.TryReadSExpGolomb(out var offset_for_top_to_bottom_field)) != ReaderError.None)
                        return (SPSError)rError;

                    OffsetForTopToBottomField = offset_for_top_to_bottom_field;

                    if ((rError = spsReader.TryReadUExpGolomb(out var num_ref_frames_in_pic_order_cnt_cycle)) != ReaderError.None)
                        return (SPSError)rError;

                    if (num_ref_frames_in_pic_order_cnt_cycle > 255)
                        return SPSError.InvalidNumRefFramesInCycle;

                    NumRefFramesInCycle = (byte)num_ref_frames_in_pic_order_cnt_cycle;
                    if (num_ref_frames_in_pic_order_cnt_cycle > 0)
                    {
                        OffsetRefFrames = (int*)UnsafeUtility.Malloc(num_ref_frames_in_pic_order_cnt_cycle * sizeof(int), 4, Allocator);

                        for (int i = 0; i < num_ref_frames_in_pic_order_cnt_cycle; i++)
                        {
                            if ((rError = spsReader.TryReadSExpGolomb(out var offsetForRefFrame)) != ReaderError.None)
                                return (SPSError)rError;

                            OffsetRefFrames[i] = offsetForRefFrame;
                        }
                    }
                    break;
            }

            if ((rError = spsReader.TryReadUExpGolomb(out var max_num_ref_frames)) != ReaderError.None)
                return (SPSError)rError;

            if (max_num_ref_frames > 16)
                return SPSError.InvalidMaxNumRefFrames;

            MaxNumRefFrames = (int)max_num_ref_frames;

            if ((rError = spsReader.TryReadBool(out var gaps_in_frame_num_value_allowed)) != ReaderError.None)
                return (SPSError)rError;

            GapsInFrameNumValueAllowed = gaps_in_frame_num_value_allowed;

            if ((rError = spsReader.TryReadUExpGolomb(out var pic_width_in_mbs_minus_1)) != ReaderError.None)
                return (SPSError)rError;

            if (pic_width_in_mbs_minus_1 + 1 >= ushort.MaxValue)
                return SPSError.InvalidMbHeight;

            MbWidth = pic_width_in_mbs_minus_1 + 1;

            if ((rError = spsReader.TryReadUExpGolomb(out var pic_height_in_map_units_minus_1)) != ReaderError.None)
                return (SPSError)rError;

            if (pic_height_in_map_units_minus_1 + 1 >= ushort.MaxValue)
                return SPSError.InvalidMbHeight;

            MbHeigth = pic_height_in_map_units_minus_1 + 1;

            if ((rError = spsReader.TryReadBool(out var frame_mbs_only)) != ReaderError.None)
                return (SPSError)rError;

            FrameMbsOnly = frame_mbs_only;

            if (!frame_mbs_only)
            {
                MbHeigth *= 2;

                if ((rError = spsReader.TryReadBool(out var mb_adaptive_frame_field)) != ReaderError.None)
                    return (SPSError)rError;

                MbAdaptiveFrameField = mb_adaptive_frame_field;
            }

            if ((rError = spsReader.TryReadBool(out var direct_8x8_inference)) != ReaderError.None)
                return (SPSError)rError;

            Direct8x8Inference = direct_8x8_inference;

            if ((rError = spsReader.TryReadBool(out var frame_cropping_present)) != ReaderError.None)
                return (SPSError)rError;

            if (frame_cropping_present)
            {
                var cropUnitX = ArrayType == 0 ? 1 : SubWidthC;
                var cropUnitY = ArrayType == 0 ? 1 : SubHeightC;

                cropUnitY *= frame_mbs_only ? 1u : 2u;

                if ((rError = spsReader.TryReadUExpGolomb(out var frame_crop_left_offset)) != ReaderError.None)
                    return (SPSError)rError;

                CropLeft = frame_crop_left_offset * cropUnitX;

                if ((rError = spsReader.TryReadUExpGolomb(out var frame_crop_right_offset)) != ReaderError.None)
                    return (SPSError)rError;

                CropRight = frame_crop_right_offset * cropUnitX;

                if ((rError = spsReader.TryReadUExpGolomb(out var frame_crop_top_offset)) != ReaderError.None)
                    return (SPSError)rError;

                CropTop = frame_crop_top_offset * cropUnitY;

                if ((rError = spsReader.TryReadUExpGolomb(out var frame_crop_bottom_offset)) != ReaderError.None)
                    return (SPSError)rError;

                CropBottom = frame_crop_bottom_offset * cropUnitY;

                if (CropLeft + CropRight > PictureWidth || CropTop + CropBottom > PictureHeight)
                    return SPSError.InvalidCrop;
            }

            if ((rError = spsReader.TryReadBool(out var vui_parameters_present)) != ReaderError.None)
                return (SPSError)rError;

            if (vui_parameters_present)
            {
                if ((rError = spsReader.TryReadBool(out var aspect_ratio_info_present)) != ReaderError.None)
                    return (SPSError)rError;

                if (aspect_ratio_info_present)
                {
                    if ((rError = spsReader.TryReadBits(8, out var aspect_ratio_idc)) != ReaderError.None)
                        return (SPSError)rError;

                    if (aspect_ratio_idc > 16 && aspect_ratio_idc != 255)
                        return SPSError.InvalidAspectIndicator;

                    const int Extend_SAR = 255;
                    if (aspect_ratio_idc == Extend_SAR)
                    {
                        if ((rError = spsReader.TryReadBits(16, out var aspect_ratio_width)) != ReaderError.None)
                            return (SPSError)rError;

                        if (aspect_ratio_width > ushort.MaxValue)
                            return SPSError.InvalidAspectExtendedValue;

                        SAR.Num = (int)aspect_ratio_width;

                        if ((rError = spsReader.TryReadBits(16, out var aspect_ratio_height)) != ReaderError.None)
                            return (SPSError)rError;

                        if (aspect_ratio_height > ushort.MaxValue)
                            return SPSError.InvalidAspectExtendedValue;

                        SAR.Denom = (int)aspect_ratio_height;
                    }
                    else
                    {
                        SAR = H264Utility.GetSARFromIndicator((byte)aspect_ratio_idc);
                    }
                }

                if ((rError = spsReader.TryReadBool(out var overscan_info_present)) != ReaderError.None)
                    return (SPSError)rError;

                if (overscan_info_present)
                {
                    if ((rError = spsReader.TryReadBool(out var overscan_appropriate)) != ReaderError.None)
                        return (SPSError)rError;
                }

                if ((rError = spsReader.TryReadBool(out var video_signal_type_present)) != ReaderError.None)
                    return (SPSError)rError;

                if (video_signal_type_present)
                {
                    if ((rError = spsReader.TryReadBits(3, out var video_format)) != ReaderError.None)
                        return (SPSError)rError;

                    if ((rError = spsReader.TryReadBool(out var video_full_range)) != ReaderError.None)
                        return (SPSError)rError;

                    VideoFullRange = video_full_range;

                    if ((rError = spsReader.TryReadBool(out var colour_description_present)) != ReaderError.None)
                        return (SPSError)rError;

                    if (colour_description_present)
                    {
                        if (!spsReader.HasEnoughBits(24))
                            return SPSError.ReaderOutOfRange;

                        if ((rError = spsReader.TryReadBits(8, out var colour_primaries)) != ReaderError.None)
                            return (SPSError)rError;

                        if ((rError = spsReader.TryReadBits(8, out var transfer_characteristics)) != ReaderError.None)
                            return (SPSError)rError;

                        if ((rError = spsReader.TryReadBits(8, out var matrix_coefficients)) != ReaderError.None)
                            return (SPSError)rError;

                        ColourPrimaries = (ColorPrimaries)colour_primaries;
                        TransferCharacteristics = (ColorTransferCharacteristic)transfer_characteristics;
                        MatrixCoefficients = (ColorMatrix)matrix_coefficients;
                    }
                }

                if ((rError = spsReader.TryReadBool(out var chroma_loc_info_present)) != ReaderError.None)
                    return (SPSError)rError;

                if (chroma_loc_info_present)
                {
                    if ((rError = spsReader.TryReadUExpGolomb(out var chroma_sample_loc_type_top_field)) != ReaderError.None)
                        return (SPSError)rError;

                    if (chroma_sample_loc_type_top_field > 5)
                        return SPSError.InvalidChromaLocTypeField;

                    ChromaLoc.TopField = (byte)chroma_sample_loc_type_top_field;

                    if ((rError = spsReader.TryReadUExpGolomb(out var chroma_sample_loc_type_bottom_field)) != ReaderError.None)
                        return (SPSError)rError;

                    if (chroma_sample_loc_type_bottom_field > 5)
                        return SPSError.InvalidChromaLocTypeField;

                    ChromaLoc.BottomField = (byte)chroma_sample_loc_type_bottom_field;
                }

                if ((rError = spsReader.TryReadBool(out var timing_info_present)) != ReaderError.None)
                    return (SPSError)rError;

                if (timing_info_present)
                {
                    if ((rError = spsReader.TryReadBits(32, out var num_units_in_tick)) != ReaderError.None)
                        return (SPSError)rError;

                    if (num_units_in_tick == 0)
                        return SPSError.InvalidTimeInfo;

                    NumUnitsInTick = num_units_in_tick;

                    if ((rError = spsReader.TryReadBits(32, out var time_scale)) != ReaderError.None)
                        return (SPSError)rError;

                    if (time_scale == 0)
                        return SPSError.InvalidTimeInfo;

                    Timescale = time_scale;

                    if ((rError = spsReader.TryReadBool(out var fixed_frame_rate)) != ReaderError.None)
                        return (SPSError)rError;

                    FixedFrameRate = fixed_frame_rate;
                }

                // Done parsing for now. Remaining parameters
                // nal_hrd_parameters
                // vcl_hrd_parameters
                // bitstream_restriction
            }

            return SPSError.None;
        }

        private unsafe static int CopyAndSanitizeSPSData(byte* dest, byte* src, int length)
        {
            int newLength = 0, i = 0;
            while (i + 2 < length)
            {
                if (src[i] == 0 && src[i + 1] == 0 && src[i + 2] == 3)
                {
                    dest[newLength++] = src[i++];
                    dest[newLength++] = src[i++];
                    i++;
                }
                else
                    dest[newLength++] = src[i++];
            }

            while (i < length)
                dest[newLength++] = src[i++];

            return newLength;
        }

        public void Dispose()
        {
            if (Allocator == Allocator.Invalid || Allocator == Allocator.None)
                return;

            if (ScalingMatrix.m_Buffer != null)
                UnsafeUtility.Free(ScalingMatrix.m_Buffer, Allocator);

            if (OffsetRefFrames != null)
                UnsafeUtility.Free(OffsetRefFrames, Allocator);
        }
    }
}
