using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class MDHDTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidVersion0_AllValueAreEqual()
        {
            fixed (byte* ptr = mdhdSmallVideoVersion0)
            {
                var reader = new BByteReader(ptr, mdhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Errors, "Logger.Errors");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(267264, track.Duration, "Duration");
                Assert.AreEqual(48000, track.Timescale, "Timescale");
                Assert.AreEqual(ISOLanguage.ENG, track.Language, "Language");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_ValidVersion1_AllValueAreEqual()
        {
            fixed (byte* ptr = mdhdSmallVideoVersion1)
            {
                var reader = new BByteReader(ptr, mdhdSmallVideoVersion1.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Errors, "Logger.Errors");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(267264, track.Duration, "Duration");
                Assert.AreEqual(48000, track.Timescale, "Timescale");
                Assert.AreEqual(ISOLanguage.ENG, track.Language, "Language");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = mdhdSmallVideoVersion0)
            {
                ref var track = ref context.LastTrack;
                track.Duration = 1;

                var reader = new BByteReader(ptr, mdhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = mdhdSmallVideoVersion0)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, mdhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion0Size_InvalidBoxSize()
        {
            fixed (byte* ptr = mdhdSmallVideoVersion0)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, mdhdSmallVideoVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = MDHD.Version0 + 2;

                var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion1Size_InvalidBoxSize()
        {
            fixed (byte* ptr = mdhdSmallVideoVersion1)
            {
                ref var track = ref context.LastTrack;

                var reader = new BByteReader(ptr, mdhdSmallVideoVersion1.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = MDHD.Version1 + 2;

                var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger.Errors");
            }
        }

        [Test]
        public unsafe void Read_BoxVersion3_InvalidBoxVersion()
        {
            byte* ptr = stackalloc byte[mdhdSmallVideoVersion0.Length];
            for (int i = 0; i < mdhdSmallVideoVersion0.Length; i++)
                ptr[i] = mdhdSmallVideoVersion0[i];

            ptr[8] = 3; // Change the version to 3

            ref var track = ref context.LastTrack;

            var reader = new BByteReader(ptr, mdhdSmallVideoVersion0.Length, Allocator.None);

            var isoBox = reader.ReadISOBox();
            var error = MDHD.Read(ref context, ref reader, ref logger, isoBox);

            Assert.AreEqual(MP4Error.InvalidBoxVersion, error, "Error");
            Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        }

        static readonly byte[] mdhdSmallVideoVersion0 = {
	        // Offset 0x0005D38A to 0x0005D3A9 small.mp4
	        0x00, 0x00, 0x00, 0x20, 0x6D, 0x64, 0x68, 0x64, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0xCA, 0xEE, 0xA7, 0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x00, 0xBB, 0x80,
            0x00, 0x04, 0x14, 0x00, 0x15, 0xC7, 0x00, 0x00
        };

        static readonly byte[] mdhdSmallVideoVersion1 = {
	        // Created manually
	        0x00, 0x00, 0x00, 0x2C, 0x6D, 0x64, 0x68, 0x64, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0xC7, 0xCA, 0xEE, 0xA7, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x00, 0xBB, 0x80, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x04, 0x14, 0x00, 0x15, 0xC7, 0x00, 0x00
        };
    }
}


