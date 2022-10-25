using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine.TestTools;

namespace MP4.Boxes
{
    public class STCOTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_Valid_AllValueAreEqual()
        {
            fixed (byte* ptr = stcoSmallVideo)
            {
                var reader = new BByteReader(ptr, stcoSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STCO.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(stcoChunkOffsets.Length, track.STCO.Length, "STCO.Length");
                for (int i = 0; i < track.STCO.Length; i++)
                {
                    Assert.AreEqual(stcoChunkOffsets[i], track.STCO.Ptr[i].value, $"STCO.Ptr[{i}]");
                }

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = stcoSmallVideo)
            {
                ref var track = ref context.LastTrack;

                track.STCO.Length = 1;

                var reader = new BByteReader(ptr, stcoSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STCO.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = stcoSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, stcoSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = STCO.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneSample_InvalidEntryCount()
        {
            fixed (byte* ptr = stcoSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, stcoSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size += STCO.SampleSize;

                var error = STCO.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidEntryCount, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneByte_InvalidBoxSize()
        {
            fixed (byte* ptr = stcoSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, stcoSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = STCO.MinSize + 1;

                var error = STCO.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        static readonly byte[] stcoSmallVideo = {
	        // Offset 0x0005D1A0 to 0x0005D257 small.mp4
	        0x00, 0x00, 0x00, 0xB8, 0x73, 0x74, 0x63, 0x6F, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x00, 0xA8, 0x00, 0x00, 0x73, 0xE6,
            0x00, 0x00, 0x8B, 0xF4, 0x00, 0x00, 0xA4, 0x08, 0x00, 0x00, 0xB7, 0x64,
            0x00, 0x00, 0xC8, 0xA5, 0x00, 0x00, 0xD7, 0xD8, 0x00, 0x00, 0xE4, 0xA5,
            0x00, 0x00, 0xEC, 0x5F, 0x00, 0x00, 0xF8, 0x5F, 0x00, 0x01, 0x04, 0x41,
            0x00, 0x01, 0x1F, 0xC3, 0x00, 0x01, 0x51, 0x85, 0x00, 0x01, 0x84, 0x7E,
            0x00, 0x01, 0xCC, 0xA6, 0x00, 0x02, 0x03, 0xC0, 0x00, 0x02, 0x3C, 0x52,
            0x00, 0x02, 0x66, 0x75, 0x00, 0x02, 0x8A, 0x1C, 0x00, 0x02, 0xB4, 0xA6,
            0x00, 0x02, 0xE7, 0x66, 0x00, 0x03, 0x23, 0x01, 0x00, 0x03, 0x5D, 0xAC,
            0x00, 0x03, 0x9E, 0x97, 0x00, 0x03, 0xEA, 0x64, 0x00, 0x04, 0x26, 0x0A,
            0x00, 0x04, 0x3E, 0x69, 0x00, 0x04, 0x4C, 0xFF, 0x00, 0x04, 0x63, 0xD9,
            0x00, 0x04, 0x7E, 0x43, 0x00, 0x04, 0x98, 0x9B, 0x00, 0x04, 0xAD, 0xA9,
            0x00, 0x04, 0xBE, 0xF7, 0x00, 0x04, 0xD7, 0x94, 0x00, 0x04, 0xED, 0x6C,
            0x00, 0x05, 0x0E, 0x3B, 0x00, 0x05, 0x2D, 0xC9, 0x00, 0x05, 0x59, 0xF0,
            0x00, 0x05, 0x7B, 0x82, 0x00, 0x05, 0x95, 0x2B, 0x00, 0x05, 0xB3, 0xDA,
            0x00, 0x05, 0xC6, 0x67
        };

        static readonly uint[] stcoChunkOffsets = { 
            168, 29670, 35828, 41992, 46948, 51365, 55256, 58533, 60511, 63583, 
            66625, 73667, 86405, 99454, 117926, 132032, 146514, 157301, 166428, 
            177318, 190310, 205569, 220588, 237207, 256612, 271882, 278121, 
            281855, 287705, 294467, 301211, 306601, 311031, 317332, 322924, 
            331323, 339401, 350704, 359298, 365867, 373722, 378471 
        };
    }
}
