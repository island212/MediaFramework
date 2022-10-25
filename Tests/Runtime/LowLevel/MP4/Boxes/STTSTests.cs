using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using MediaFramework.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class STTSTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_Valid_AllValueAreEqual()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                var reader = new BByteReader(ptr, sttsSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STTS.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(sttsSmallSampleCounts.Length, track.STTS.Length, "STTS.Length");
                for (int i = 0; i < track.STTS.Length; i++)
                {
                    Assert.AreEqual(sttsSmallSampleCounts[i], track.STTS.Ptr[i].count, $"STTS.Ptr[{i}].count");
                    Assert.AreEqual(sttsSmallSampleDelta[i], track.STTS.Ptr[i].delta, $"STTS.Ptr[{i}].delta");
                }

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.LastTrack;

                track.STTS.Length = 1;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STTS.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length, Allocator.None);;

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = STTS.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneSample_InvalidEntryCount()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length, Allocator.None);;

                var isoBox = reader.ReadISOBox();
                isoBox.size += STTS.SampleSize;

                var error = STTS.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidEntryCount, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneByte_InvalidBoxSize()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length, Allocator.None);;

                var isoBox = reader.ReadISOBox();
                isoBox.size = STTS.MinSize + 1;

                var error = STTS.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        static readonly byte[] sttsSmallVideo = {
	        // Offset 0x0005CEB4 to 0x0005CECB small.mp4
	        0x00, 0x00, 0x00, 0x18, 0x73, 0x74, 0x74, 0x73, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xA6, 0x00, 0x00, 0x0B, 0xB8
        };

        static readonly uint[] sttsSmallSampleCounts = { 166 };
        static readonly uint[] sttsSmallSampleDelta = { 3000 };
    }
}
