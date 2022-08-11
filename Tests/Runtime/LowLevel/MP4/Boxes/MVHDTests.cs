﻿using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class MVHDTests
    {
        readonly byte[] mvhdSmallVersion0 = {
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

        readonly byte[] mvhdSmallVersion1 = {
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

        private MP4JobContext context;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            context = new MP4JobContext();
            context.Logger = new JobLogger(16, Allocator.Temp);
            context.Tracks = new UnsafeList<TRAKBox>(1, Allocator.Temp);
        }

        [TearDown]
        public void TearDown()
        {
            var logger = context.Logger;
            var tracks = context.Tracks;

            logger.Clear();
            tracks.Clear();

            context = new MP4JobContext();
            context.Logger = logger;
            context.Tracks = tracks;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            context.Logger.Dispose();
            context.Tracks.Dispose();
        }

        [Test]
        public unsafe void Read_ValidVersion0_AllValueAreEqual()
        {
            fixed (byte* ptr = mvhdSmallVersion0)
            {
                var reader = new BByteReader(ptr, mvhdSmallVersion0.Length);

                var isoBox = reader.ReadISOBox();
                var error = MVHDBox.Read(ref context, ref reader, isoBox);

                for (int i = 0; i < context.Logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

                Assert.AreEqual(90000, context.MVHD.Timescale, "TimeScale");
                Assert.AreEqual(501120, context.MVHD.Duration, "Duration");
                Assert.AreEqual(3, context.MVHD.NextTrackID, "NextTrackID");
            }
        }

        [Test]
        public unsafe void Read_ValidVersion1_AllValueAreEqual()
        {
            fixed (byte* ptr = mvhdSmallVersion1)
            {
                var reader = new BByteReader(ptr, mvhdSmallVersion1.Length);

                var isoBox = reader.ReadISOBox();
                var error = MVHDBox.Read(ref context, ref reader, isoBox);

                for (int i = 0; i < context.Logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

                Assert.AreEqual(90000, context.MVHD.Timescale, "TimeScale");
                Assert.AreEqual(501120, context.MVHD.Duration, "Duration");
                Assert.AreEqual(3, context.MVHD.NextTrackID, "NextTrackID");
            }
        }

        [Test]
        public unsafe void Read_Duplicate_ReturnErrorAndLog()
        {
            fixed (byte* ptr = mvhdSmallVersion0)
            {
                context.MVHD.Duration = 60;
                context.MVHD.Timescale = 1;

                var reader = new BByteReader(ptr, mvhdSmallVersion0.Length);

                var isoBox = reader.ReadISOBox();
                var error = MVHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_ReturnErrorAndLog()
        {
            fixed (byte* ptr = mvhdSmallVersion0)
            {
                var reader = new BByteReader(ptr, mvhdSmallVersion0.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = 0;

                var error = MVHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_WrongBoxSizeForVersion0_ReturnErrorAndLog()
        {
            fixed (byte* ptr = mvhdSmallVersion0)
            {
                var reader = new BByteReader(ptr, mvhdSmallVersion0.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = MVHDBox.Version0 + 2;

                var error = MVHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_WrongBoxSizeForVersion1_ReturnErrorAndLog()
        {
            fixed (byte* ptr = mvhdSmallVersion1)
            {
                var reader = new BByteReader(ptr, mvhdSmallVersion1.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = MVHDBox.Version1 + 2;

                var error = MVHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion3_ReturnErrorAndLog()
        {
            byte* ptr = stackalloc byte[mvhdSmallVersion0.Length];
            for (int i = 0; i < mvhdSmallVersion0.Length; i++)
                ptr[i] = mvhdSmallVersion0[i];

            ptr[8] = 3; // Change the version to 3

            var reader = new BByteReader(ptr, mvhdSmallVersion0.Length);

            var isoBox = reader.ReadISOBox();
            var error = MVHDBox.Read(ref context, ref reader, isoBox);

            Assert.AreEqual(MP4Error.InvalidBoxVersion, error, "Error");
            Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
        }
    }
}
