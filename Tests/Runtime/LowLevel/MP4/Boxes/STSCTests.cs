﻿using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace MP4.Boxes
{
    public class STSCTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_Valid_AllValueAreEqual()
        {
            fixed (byte* ptr = stscSmallVideo)
            {
                var reader = new BByteReader(ptr, stscSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STSCBox.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(2, track.STSC.EntryCount, "EntryCount");
                Assert.AreEqual(16, track.STSC.SampleIndex, "SampleIndex");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = stscSmallVideo)
            {
                ref var track = ref context.LastTrack;
                track.STSC.EntryCount = 1;

                var reader = new BByteReader(ptr, stscSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STSCBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = stscSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, stscSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = STSCBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneSample_InvalidEntryCount()
        {
            fixed (byte* ptr = stscSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, stscSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size += STSCBox.SampleSize;

                var error = STSCBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidEntryCount, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneByte_InvalidBoxSize()
        {
            fixed (byte* ptr = stscSmallVideo)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, stscSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = STSCBox.MinSize + 1;

                var error = STSCBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        static readonly byte[] stscSmallVideo = {
	        // Offset 0x0005D178 to 0x0005D19F small.mp4
	        0x00, 0x00, 0x00, 0x28, 0x73, 0x74, 0x73, 0x63, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x02,
            0x00, 0x00, 0x00, 0x01
        };
    }
}
