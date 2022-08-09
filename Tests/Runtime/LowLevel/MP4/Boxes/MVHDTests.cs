using MediaFramework.LowLevel;
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
        public unsafe void Read_ValidMVHD_AllValueAreEqual()
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

                Assert.AreEqual(90000, context.MVHD.TimeScale, "TimeScale");
                Assert.AreEqual(501120, context.MVHD.Duration, "Duration");
                Assert.AreEqual(3, context.MVHD.NextTrackID, "NextTrackID");
            }
        }
    }
}
