using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class TKHDTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidVersion0_AllValueAreEqual()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.Track;

                Assert.AreEqual(1, track.TrackID, "TrackID");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_ValidVersion1_AllValueAreEqual()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion1)
            {
                var reader = new BByteReader(ptr, tkhdSmallVideoVersion1.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.Track;

                Assert.AreEqual(1, track.TrackID, "TrackID");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                ref var track = ref context.Track;
                track.TrackID = 1;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                ref var track = ref context.Track;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion0Size_InvalidBoxSize()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                ref var track = ref context.Track;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = TKHDBox.Version0 + 2;

                var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion1Size_InvalidBoxSize()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion1)
            {
                ref var track = ref context.Track;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion1.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = TKHDBox.Version1 + 2;

                var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxVersion3_InvalidBoxVersion()
        {
            byte* ptr = stackalloc byte[tkhdSmallVideoVersion0.Length];
            for (int i = 0; i < tkhdSmallVideoVersion0.Length; i++)
                ptr[i] = tkhdSmallVideoVersion0[i];

            ptr[8] = 3; // Change the version to 3

            ref var track = ref context.Track;

            var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length, Allocator.None);

            var isoBox = reader.ReadISOBox();
            var error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox);

            Assert.AreEqual(MP4Error.InvalidBoxVersion, error, "Error");
            Assert.AreEqual(1, logger.Length, "Logger.Length");
        }

        readonly byte[] tkhdSmallVideoVersion0 = {
	        // Offset 0x0005CD1C to 0x0005CD77 small.mp4
	        0x00, 0x00, 0x00, 0x5C, 0x74, 0x6B, 0x68, 0x64, 0x00, 0x00, 0x00, 0x01,
            0xC7, 0xCA, 0xEE, 0xA7, 0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x99, 0x50, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00,
            0x02, 0x30, 0x00, 0x00, 0x01, 0x40, 0x00, 0x00
        };

        readonly byte[] tkhdSmallVideoVersion1 = {
	        // Created manually
	        0x00, 0x00, 0x00, 0x68, 0x74, 0x6B, 0x68, 0x64, 0x01, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x00, 0xC7, 0xCA, 0xEE, 0xA7, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x99, 0x50, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00,
            0x02, 0x30, 0x00, 0x00, 0x01, 0x40, 0x00, 0x00
        };
    }
}


