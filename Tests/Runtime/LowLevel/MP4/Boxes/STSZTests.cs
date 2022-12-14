using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine.TestTools;

namespace MP4.Boxes
{
    public class STSZTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_SampleSize0_AllValueAreEqual()
        {
            fixed (byte* ptr = STSZBufferSampleSize0)
            {
                var reader = new BByteReader(ptr, STSZBufferSampleSize0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STSZ.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Errors, "Logger.Errors");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(0, track.STSZ.SampleSize, "SampleSize");
                Assert.AreEqual(166, track.STSZ.Length, "Length");
                for (int i = 0; i < track.STSZ.Length; i++)
                {
                    Assert.AreEqual(SampleSizes[i], track.STSZ.Samples[i], $"Samples[{i}]");
                }
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_SampleSizeNot0_AllValueAreEqual()
        {
            fixed (byte* ptr = STSZBufferSampleSizeNot0)
            {
                var reader = new BByteReader(ptr, STSZBufferSampleSizeNot0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STSZ.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Errors, "Logger.Errors");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(22055, track.STSZ.SampleSize, "SampleSize");
                Assert.AreEqual(166, track.STSZ.Length, "Length");
                Assert.IsTrue(track.STSZ.Samples == null, "Samples");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = STSZBufferSampleSize0)
            {
                ref var track = ref context.LastTrack;

                track.STSZ.Length = 1;

                var reader = new BByteReader(ptr, STSZBufferSampleSize0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STSZ.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = STSZBufferSampleSize0)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, STSZBufferSampleSize0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = STSZ.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneSample_InvalidEntryCount()
        {
            fixed (byte* ptr = STSZBufferSampleSize0)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, STSZBufferSampleSize0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size += STSZ.SampleSize;

                var error = STSZ.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidEntryCount, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneByte_InvalidBoxSize()
        {
            fixed (byte* ptr = STSZBufferSampleSize0)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, STSZBufferSampleSize0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = STSZ.MinSize + 1;

                var error = STSZ.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidEntryCount, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        static readonly byte[] STSZBufferSampleSize0 = {
	        // Offset 0x0005CECC to 0x0005D177 small.mp4
	        0x00, 0x00, 0x02, 0xAC, 0x73, 0x74, 0x73, 0x7A, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA6, 0x00, 0x00, 0x56, 0x27,
            0x00, 0x00, 0x0B, 0x20, 0x00, 0x00, 0x05, 0xBC, 0x00, 0x00, 0x05, 0xE2,
            0x00, 0x00, 0x05, 0xC1, 0x00, 0x00, 0x04, 0x37, 0x00, 0x00, 0x04, 0x07,
            0x00, 0x00, 0x03, 0xB6, 0x00, 0x00, 0x06, 0x45, 0x00, 0x00, 0x03, 0x73,
            0x00, 0x00, 0x05, 0x12, 0x00, 0x00, 0x03, 0x26, 0x00, 0x00, 0x02, 0xE9,
            0x00, 0x00, 0x03, 0x7B, 0x00, 0x00, 0x03, 0x4A, 0x00, 0x00, 0x03, 0x6B,
            0x00, 0x00, 0x02, 0xB6, 0x00, 0x00, 0x03, 0x4C, 0x00, 0x00, 0x02, 0x7A,
            0x00, 0x00, 0x02, 0xC7, 0x00, 0x00, 0x02, 0x2E, 0x00, 0x00, 0x03, 0x16,
            0x00, 0x00, 0x02, 0x26, 0x00, 0x00, 0x02, 0x7F, 0x00, 0x00, 0x01, 0xEC,
            0x00, 0x00, 0x01, 0xEA, 0x00, 0x00, 0x01, 0xF5, 0x00, 0x00, 0x01, 0xEB,
            0x00, 0x00, 0x01, 0xFA, 0x00, 0x00, 0x01, 0xE7, 0x00, 0x00, 0x01, 0xFC,
            0x00, 0x00, 0x01, 0xDD, 0x00, 0x00, 0x01, 0xC6, 0x00, 0x00, 0x01, 0xAE,
            0x00, 0x00, 0x01, 0xC8, 0x00, 0x00, 0x01, 0xB9, 0x00, 0x00, 0x01, 0x90,
            0x00, 0x00, 0x01, 0x93, 0x00, 0x00, 0x01, 0x8C, 0x00, 0x00, 0x01, 0xDA,
            0x00, 0x00, 0x01, 0xC2, 0x00, 0x00, 0x05, 0xD0, 0x00, 0x00, 0x07, 0xB8,
            0x00, 0x00, 0x06, 0x7A, 0x00, 0x00, 0x09, 0xA9, 0x00, 0x00, 0x0A, 0x2C,
            0x00, 0x00, 0x0A, 0x7C, 0x00, 0x00, 0x0C, 0xB3, 0x00, 0x00, 0x09, 0x8C,
            0x00, 0x00, 0x09, 0x52, 0x00, 0x00, 0x0C, 0x04, 0x00, 0x00, 0x0D, 0xC1,
            0x00, 0x00, 0x0F, 0x74, 0x00, 0x00, 0x10, 0x48, 0x00, 0x00, 0x11, 0x06,
            0x00, 0x00, 0x10, 0x61, 0x00, 0x00, 0x0C, 0x63, 0x00, 0x00, 0x0C, 0x31,
            0x00, 0x00, 0x0B, 0x42, 0x00, 0x00, 0x0C, 0x0D, 0x00, 0x00, 0x0F, 0x32,
            0x00, 0x00, 0x0A, 0x7B, 0x00, 0x00, 0x0D, 0x0F, 0x00, 0x00, 0x0A, 0xE0,
            0x00, 0x00, 0x0A, 0x0E, 0x00, 0x00, 0x0B, 0x6B, 0x00, 0x00, 0x08, 0x74,
            0x00, 0x00, 0x0C, 0x36, 0x00, 0x00, 0x09, 0xE6, 0x00, 0x00, 0x06, 0x8D,
            0x00, 0x00, 0x04, 0xF8, 0x00, 0x00, 0x07, 0x8A, 0x00, 0x00, 0x07, 0xC1,
            0x00, 0x00, 0x09, 0xF3, 0x00, 0x00, 0x07, 0xC7, 0x00, 0x00, 0x0A, 0xCB,
            0x00, 0x00, 0x0A, 0xD2, 0x00, 0x00, 0x0B, 0x74, 0x00, 0x00, 0x0C, 0x28,
            0x00, 0x00, 0x0A, 0x9A, 0x00, 0x00, 0x0C, 0x60, 0x00, 0x00, 0x0D, 0x6D,
            0x00, 0x00, 0x0C, 0x3E, 0x00, 0x00, 0x0F, 0xFC, 0x00, 0x00, 0x0E, 0x82,
            0x00, 0x00, 0x0B, 0x79, 0x00, 0x00, 0x0D, 0xE4, 0x00, 0x00, 0x0D, 0x24,
            0x00, 0x00, 0x0A, 0x17, 0x00, 0x00, 0x11, 0xAA, 0x00, 0x00, 0x12, 0x65,
            0x00, 0x00, 0x0D, 0x7B, 0x00, 0x00, 0x12, 0xA0, 0x00, 0x00, 0x13, 0xD8,
            0x00, 0x00, 0x11, 0x49, 0x00, 0x00, 0x0E, 0x59, 0x00, 0x00, 0x10, 0x15,
            0x00, 0x00, 0x16, 0x81, 0x00, 0x00, 0x09, 0xB4, 0x00, 0x00, 0x06, 0xEB,
            0x00, 0x00, 0x05, 0xEF, 0x00, 0x00, 0x05, 0x8A, 0x00, 0x00, 0x03, 0xD7,
            0x00, 0x00, 0x04, 0x0D, 0x00, 0x00, 0x03, 0xBB, 0x00, 0x00, 0x04, 0x6B,
            0x00, 0x00, 0x03, 0x40, 0x00, 0x00, 0x03, 0x30, 0x00, 0x00, 0x02, 0xDE,
            0x00, 0x00, 0x03, 0xAE, 0x00, 0x00, 0x05, 0xCF, 0x00, 0x00, 0x04, 0x6C,
            0x00, 0x00, 0x05, 0x69, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x06, 0xA1,
            0x00, 0x00, 0x03, 0x35, 0x00, 0x00, 0x04, 0x1A, 0x00, 0x00, 0x03, 0xFA,
            0x00, 0x00, 0x06, 0x3D, 0x00, 0x00, 0x05, 0xD6, 0x00, 0x00, 0x04, 0x68,
            0x00, 0x00, 0x02, 0xD6, 0x00, 0x00, 0x04, 0xB5, 0x00, 0x00, 0x02, 0xD9,
            0x00, 0x00, 0x02, 0x7F, 0x00, 0x00, 0x02, 0x4D, 0x00, 0x00, 0x02, 0x7D,
            0x00, 0x00, 0x03, 0x8C, 0x00, 0x00, 0x02, 0x06, 0x00, 0x00, 0x02, 0x01,
            0x00, 0x00, 0x07, 0x7F, 0x00, 0x00, 0x05, 0xEF, 0x00, 0x00, 0x05, 0xB8,
            0x00, 0x00, 0x04, 0x0A, 0x00, 0x00, 0x02, 0x99, 0x00, 0x00, 0x03, 0x1D,
            0x00, 0x00, 0x07, 0xC5, 0x00, 0x00, 0x05, 0xAC, 0x00, 0x00, 0x04, 0x78,
            0x00, 0x00, 0x08, 0x71, 0x00, 0x00, 0x08, 0x99, 0x00, 0x00, 0x08, 0xE9,
            0x00, 0x00, 0x08, 0x99, 0x00, 0x00, 0x05, 0x73, 0x00, 0x00, 0x07, 0xC7,
            0x00, 0x00, 0x08, 0x3D, 0x00, 0x00, 0x0B, 0x59, 0x00, 0x00, 0x0A, 0x36,
            0x00, 0x00, 0x06, 0xBA, 0x00, 0x00, 0x05, 0xF9, 0x00, 0x00, 0x07, 0x2E,
            0x00, 0x00, 0x06, 0xEB, 0x00, 0x00, 0x04, 0xC6, 0x00, 0x00, 0x04, 0xBA,
            0x00, 0x00, 0x05, 0x66, 0x00, 0x00, 0x04, 0x31, 0x00, 0x00, 0x06, 0x8A,
            0x00, 0x00, 0x06, 0xCF, 0x00, 0x00, 0x06, 0xFE, 0x00, 0x00, 0x04, 0x97,
            0x00, 0x00, 0x02, 0x43, 0x00, 0x00, 0x03, 0xE2, 0x00, 0x00, 0x04, 0x06,
            0x00, 0x00, 0x02, 0xE6, 0x00, 0x00, 0x02, 0x6B, 0x00, 0x00, 0x02, 0x75
        };

        static readonly byte[] STSZBufferSampleSizeNot0 = {
	        // Created manually
	        0x00, 0x00, 0x02, 0xAC, 0x73, 0x74, 0x73, 0x7A, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x56, 0x27, 0x00, 0x00, 0x00, 0xA6
        };

        static readonly uint[] SampleSizes = {
            22055,2848,1468,1506,1473,1079,1031,950,1605,883,1298,806,745,891,842,875,694,844,634,
            711,558,790,550,639,492,490,501,491,506,487,508,477,454,430,456,441,400,403,396,474,450,
            1488,1976,1658,2473,2604,2684,3251,2444,2386,3076,3521,3956,4168,4358,4193,3171,3121,
            2882,3085,3890,2683,3343,2784,2574,2923,2164,3126,2534,1677,1272,1930,1985,2547,1991,
            2763,2770,2932,3112,2714,3168,3437,3134,4092,3714,2937,3556,3364,2583,4522,4709,3451,
            4768,5080,4425,3673,4117,5761,2484,1771,1519,1418,983,1037,955,1131,832,816,734,942,1487,
            1132,1385,1280,1697,821,1050,1018,1597,1494,1128,726,1205,729,639,589,637,908,518,513,
            1919,1519,1464,1034,665,797,1989,1452,1144,2161,2201,2281,2201,1395,1991,2109,2905,2614,
            1722,1529,1838,1771,1222,1210,1382,1073,1674,1743,1790,1175,579,994,1030,742,619,629
        };
    }
}
