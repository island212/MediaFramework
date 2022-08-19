using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using MediaFramework.LowLevel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace MP4.Boxes
{
    public class HDLRTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_VideoHandler_AllValueAreEqual()
        {
            fixed (byte* ptr = hdlrSmall)
            {
                var reader = new BByteReader(ptr, hdlrSmall.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = HDLRBox.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.Track;

                Assert.AreEqual(ISOHandler.VIDE, track.Handler, "Handler");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_VideoHandlerWithName_AllValueAreEqual()
        {
            byte* ptr = stackalloc byte[hdlrSmall.Length + 7];
            for (int i = 0; i < hdlrSmall.Length; i++)
                ptr[i] = hdlrSmall[i];

            var reader = new BByteReader(ptr, hdlrSmall.Length, Allocator.None);

            var isoBox = reader.ReadISOBox();
            isoBox.size = (uint)hdlrSmall.Length + 7;

            var error = HDLRBox.Read(ref context, ref reader, ref logger, isoBox);

            PrintLog();

            Assert.AreEqual(MP4Error.None, error, "Error");
            Assert.AreEqual(0, logger.Length, "Logger.Length");

            ref var track = ref context.Track;

            Assert.AreEqual(ISOHandler.VIDE, track.Handler, "Handler");
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = hdlrSmall)
            {
                ref var track = ref context.Track;
                track.Handler = ISOHandler.VIDE;

                var reader = new BByteReader(ptr, hdlrSmall.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = HDLRBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = hdlrSmall)
            {
                ref var track = ref context.Track;

                var reader = new BByteReader(ptr, hdlrSmall.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = HDLRBox.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, logger.Length, "Logger.Length");
            }
        }

        readonly byte[] hdlrSmall = {
	        // Offset 0x0005CDA0 to 0x0005CDC0 small.mp4
	        0x00, 0x00, 0x00, 0x21, 0x68, 0x64, 0x6C, 0x72, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x76, 0x69, 0x64, 0x65, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
