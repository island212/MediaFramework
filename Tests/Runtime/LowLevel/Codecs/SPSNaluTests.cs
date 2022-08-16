using System.Collections;
using System.Collections.Generic;
using MediaFramework.LowLevel.Codecs;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.TestTools;

namespace Codecs
{
    public class SPSNaluTests
    {
        readonly byte[] spsSmall =
        {
            0x67, 0x42, 0xC0, 0x1E, 0x9E, 0x21, 0x81, 0x18, 0x53, 0x4D, 0x40, 0x40,
            0x40, 0x50, 0x00, 0x00, 0x03, 0x00, 0x10, 0x00, 0x00, 0x03, 0x03, 0xC8,
            0xF1, 0x62, 0xEE
        };

        readonly byte[] spsBasic =
        {
            0x67, 0x42, 0xC0, 0x0D, 0x95, 0xB0, 0x50, 0x6F, 0xE5, 0xC0, 0x44,
            0x00, 0x00, 0x03, 0x00, 0x04, 0x00, 0x00, 0x03, 0x00, 0xF0, 0x36, 0x82,
            0x21, 0x1B
        };

        [Test]
        public unsafe void Parse_ValidSPS_AllValueAreEqual()
        {
            // Need to do a copy because we remove the emulation byte
            // and we don't want to modifiy the byte[] for other tests
            var spsDataPtr = stackalloc byte[spsSmall.Length];
            fixed (byte* ptr = spsSmall)
                UnsafeUtility.MemCpy(spsDataPtr, ptr, spsSmall.Length);

            var reader = new BByteReader(spsDataPtr, spsSmall.Length);

            using var sps = new SPSNalu();
            var error = sps.Parse(reader, Allocator.Temp);

            Assert.AreEqual(SPSError.None, error, "SPSError");

            Assert.AreEqual(66, sps.Profile.Type, "Profile");
            Assert.AreEqual(192, sps.Profile.Constraints, "Constraints");
            Assert.AreEqual(30, sps.Profile.Level, "Level");

            Assert.AreEqual(0, sps.ID, "ID");

            Assert.AreEqual(ChromaSubsampling.YUV420, sps.ChromaFormat, "ChromaFormat");
            Assert.AreEqual(8, sps.BitDepth, "BitDepth");

            Assert.IsFalse(sps.ScalingMatrix.IsCreated, "ScalingMatrix");

            Assert.AreEqual(1024, sps.MaxFrameNum, "MaxFrameNum");

            Assert.AreEqual(0, sps.POCType, "PicOrderCntType");
            Assert.AreEqual(2, sps.MaxNumRefFrames, "MaxNumRefFrames");
            Assert.AreEqual(2048, sps.MaxPOCLsb, "MaxLsb");
            Assert.AreEqual(0, sps.OffsetForNonRefPic, "OffsetForNonRefPic");
            Assert.AreEqual(0, sps.OffsetForTopToBottomField, "OffsetForTopToBottomField");
            Assert.AreEqual(0, sps.NumRefFramesInCycle, "NumRefFramesInCycle");
            Assert.IsTrue(sps.OffsetRefFrames == null, "OffsetRefFrames");

            Assert.IsFalse(sps.GapsInFrameNumValueAllowed, "GapsInFrameNumValueAllowed");
            Assert.AreEqual(35, sps.MbWidth, "MbWidth");
            Assert.AreEqual(20, sps.MbHeigth, "MbHeigth");
            Assert.IsTrue(sps.FrameMbsOnly, "FrameMbsOnly");
            Assert.IsFalse(sps.MbAdaptiveFrameField, "MbAdaptiveFrameField");
            Assert.IsTrue(sps.Direct8x8Inference, "Direct8x8Inference");

            Assert.AreEqual(0, sps.CropLeft, "CropLeft");
            Assert.AreEqual(0, sps.CropRight, "CropRight");
            Assert.AreEqual(0, sps.CropTop, "CropTop");
            Assert.AreEqual(0, sps.CropBottom, "CropBottom");

            Assert.AreEqual(0, sps.SAR.Num, "SAR Width");
            Assert.AreEqual(0, sps.SAR.Denom, "SAR Heigth");

            Assert.IsFalse(sps.VideoFullRange, "VideoFullRange");
            Assert.AreEqual(AVColorPrimaries.BT709, sps.ColourPrimaries, "ColourPrimaries");
            Assert.AreEqual(AVColorTransferCharacteristic.BT709, sps.TransferCharacteristics, "TransferCharacteristics");
            Assert.AreEqual(AVColorSpace.BT709, sps.MatrixCoefficients, "MatrixCoefficients");

            Assert.AreEqual(0, sps.ChromaLoc.TopField, "ChromaSampleLocType TopField");
            Assert.AreEqual(0, sps.ChromaLoc.BottomField, "ChromaSampleLocType BottomField");

            Assert.AreEqual(1, sps.NumUnitsInTick, "NumUnitsInTick");
            Assert.AreEqual(60, sps.Timescale, "TimeScale");
        }
    }
}