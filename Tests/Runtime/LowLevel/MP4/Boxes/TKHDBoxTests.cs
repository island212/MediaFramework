using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class TKHDBoxTests
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
            context.Logger.Clear();
            context.Tracks.Clear();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            context.Logger.Dispose();
            context.Tracks.Dispose();
        }

        [Test]
        public unsafe void Read_ValidTKHD_AllValueAreEqual()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                context.Tracks.Add(new TRAKBox());
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
    }
}


