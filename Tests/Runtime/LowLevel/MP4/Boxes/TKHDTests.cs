using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class TKHDTests
    {
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

        private MP4JobContext context;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            context = new MP4JobContext();
            context.Logger = new JobLogger(16, Allocator.Temp);
            context.Tracks = new UnsafeList<TRAKBox>(1, Allocator.Temp);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            context.Logger.Dispose();
            context.Tracks.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            context.Tracks.Add(new TRAKBox());
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

        [Test]
        public unsafe void Read_ValidVersion0_AllValueAreEqual()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);

                var isoBox = reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                for (int i = 0; i < context.Logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

                ref var track = ref context.CurrentTrack;

                Assert.AreEqual(1, track.TKHD.TrackID, "TrackID");
            }
        }

        [Test]
        public unsafe void Read_ValidVersion1_AllValueAreEqual()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion1)
            {
                var reader = new BByteReader(ptr, tkhdSmallVideoVersion1.Length);

                var isoBox = reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                for (int i = 0; i < context.Logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

                ref var track = ref context.CurrentTrack;

                Assert.AreEqual(1, track.TKHD.TrackID, "TrackID");
            }
        }

        [Test]
        public unsafe void Read_Duplicate_ReturnErrorAndLog()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                ref var track = ref context.CurrentTrack;
                track.TKHD.TrackID = 1;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);

                var isoBox = reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_ReturnErrorAndLog()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = 0;

                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_WrongBoxSizeForVersion0_ReturnErrorAndLog()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = TKHDBox.Version0 + 2;

                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_WrongBoxSizeForVersion1_ReturnErrorAndLog()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion1)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, tkhdSmallVideoVersion1.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = TKHDBox.Version1 + 2;

                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidVersion3_ReturnErrorAndLog()
        {
            byte* ptr = stackalloc byte[tkhdSmallVideoVersion0.Length];
            for (int i = 0; i < tkhdSmallVideoVersion0.Length; i++)
                ptr[i] = tkhdSmallVideoVersion0[i];

            ptr[8] = 3; // Change the version to 3

            ref var track = ref context.CurrentTrack;

            var reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);

            var isoBox = reader.ReadISOBox();
            var error = TKHDBox.Read(ref context, ref reader, isoBox);

            Assert.AreEqual(MP4Error.InvalidBoxVersion, error, "Error");
            Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
        }
    }
}


