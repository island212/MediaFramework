using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using System;
using Unity.Collections;

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

        [TearDown]
        public void TearDown()
        {
            context.Validator.Dispose();
        }

        [Test]
        public unsafe void Read_ValidTKHD_AllValueAreEqual()
        {
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                context = new MP4JobContext();
                context.Reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);
                context.Validator = new MP4Validator(16, Allocator.Temp);

                var tkhd = new TKHDBox();

                var isoBox = context.Reader.ReadISOBox();
                var error = TKHDBox.Read(ref context, ref tkhd, isoBox);

                if (context.Validator.HasError)
                {
                    foreach (var log in context.Validator.GetLogs())
                    {
                        UnityEngine.Debug.LogError(log.Message);
                    }
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.IsTrue(!context.Validator.HasError, "Validator.HasError");

                Assert.AreEqual(1, tkhd.TrackID, "TrackID");
            }
        }

        [Test]
        public unsafe void Read_InvalidReader_Fail()
        {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.Ignore();
#endif
            context = new MP4JobContext();
            context.Validator = new MP4Validator(16, Allocator.Temp);

            var tkhd = new TKHDBox();
            var isoBox = new ISOBox
            {
                Size = TKHDBox.Version0,
                Type = ISOBoxType.TKHD
            };

            Assert.Throws<ArgumentException>(() => TKHDBox.Read(ref context, ref tkhd, isoBox));
        }

        [Test]
        public unsafe void Read_InvalidValidator_Fail()
        {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.Ignore();
#endif
            fixed (byte* ptr = tkhdSmallVideoVersion0)
            {
                context = new MP4JobContext();
                context.Reader = new BByteReader(ptr, tkhdSmallVideoVersion0.Length);

                var tkhd = new TKHDBox();
                var isoBox = new ISOBox
                {
                    Size = TKHDBox.Version0,
                    Type = ISOBoxType.TKHD
                };

                Assert.Throws<ArgumentException>(() => TKHDBox.Read(ref context, ref tkhd, isoBox));
            }
        }
    }
}


