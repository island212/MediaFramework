using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;

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

        [TearDown]
        public void TearDown()
        {
            context.Validator.Dispose();
        }

        [Test]
        public unsafe void Read_ValidMVHD_AllValueAreEqual()
        {
            fixed (byte* ptr = mvhdSmallVersion0)
            {
                context = new MP4JobContext();
                context.Reader = new BByteReader(ptr, mvhdSmallVersion0.Length);
                context.Validator = new MP4Validator(16, Allocator.Temp);
                context.TrackIndex = 0;

                var mvhd = new MVHDBox();

                var isoBox = context.Reader.ReadISOBox();
                var error = MVHDBox.Read(ref context, ref mvhd, isoBox);

                if (context.Validator.HasError)
                {
                    foreach (var log in context.Validator.GetLogs())
                    {
                        UnityEngine.Debug.LogError(log);
                    }
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.IsTrue(!context.Validator.HasError, "Validator.HasError");

                Assert.AreEqual(90000, mvhd.TimeScale, "TimeScale");
                Assert.AreEqual(501120, mvhd.Duration, "Duration");
                Assert.AreEqual(3, mvhd.NextTrackID, "NextTrackID");
            }
        }
    }
}
