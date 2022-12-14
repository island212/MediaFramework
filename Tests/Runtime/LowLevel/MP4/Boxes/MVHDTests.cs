using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class MVHDTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidVersion0_AllValueAreEqual()
        {
            fixed (byte* ptr = smallVersion0)
            {
                var reader = new BByteReader(ptr, smallVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Errors, "Logger");

                Assert.AreEqual(3351965351, context.CreationTime.value, "CreationTime");

                Assert.AreEqual(90000, context.Timescale, "TimeScale");
                Assert.AreEqual(501120, context.Duration, "Duration");

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_ValidVersion1_AllValueAreEqual()
        {
            fixed (byte* ptr = smallVersion1)
            {
                var reader = new BByteReader(ptr, smallVersion1.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Errors, "Logger");

                Assert.AreEqual(3351965351, context.CreationTime.value, "CreationTime");

                Assert.AreEqual(90000, context.Timescale, "TimeScale");
                Assert.AreEqual(501120, context.Duration, "Duration");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = smallVersion0)
            {
                context.Duration = 60;
                context.Timescale = 1;

                var reader = new BByteReader(ptr, smallVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = smallVersion0)
            {
                var reader = new BByteReader(ptr, smallVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion0Size_InvalidBoxSize()
        {
            fixed (byte* ptr = smallVersion0)
            {
                var reader = new BByteReader(ptr, smallVersion0.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = MVHD.Version0 + 1;

                var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion1Size_InvalidBoxSize()
        {
            fixed (byte* ptr = smallVersion1)
            {
                var reader = new BByteReader(ptr, smallVersion1.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = MVHD.Version1 + 1;

                var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Errors, "Logger");
            }
        }

        [Test]
        public unsafe void Read_BoxVersion3_InvalidBoxVersion()
        {
            byte* ptr = stackalloc byte[smallVersion0.Length];
            for (int i = 0; i < smallVersion0.Length; i++)
                ptr[i] = smallVersion0[i];

            ptr[8] = 3; // Change the version to 3

            var reader = new BByteReader(ptr, smallVersion0.Length, Allocator.None);

            var isoBox = reader.ReadISOBox();
            var error = MVHD.Read(ref context, ref reader, ref logger, isoBox);

            Assert.AreEqual(MP4Error.InvalidBoxVersion, error, "Error");
            Assert.AreEqual(1, logger.Errors, "Logger");
        }

        static readonly byte[] smallVersion0 = {
	        // Offset 0x0005CC90 to 0x0005CCFB small.mp4
	        0x00, 0x00, 0x00, 0x6C, 0x6D, 0x76, 0x68, 0x64, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0xCA, 0xEE, 0xA7, 0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x01, 0x5F, 0x90,
            0x00, 0x07, 0xA5, 0x80, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03
        };

        static readonly byte[] smallVersion1 = {
	        // Created manually
	        0x00, 0x00, 0x00, 0x78, 0x6D, 0x76, 0x68, 0x64, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0xC7, 0xCA, 0xEE, 0xA7, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x01, 0x5F, 0x90, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x07, 0xA5, 0x80, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03
        };
    }
}
