using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using MediaFramework.LowLevel.Unsafe;
using UnityEngine.TestTools;

namespace MP4.Boxes
{
    public class VisualSampleEntryTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidVideo_AllValueAreEqual()
        {
            fixed (byte* ptr = smallVisual)
            {
                context.LastTrack.Handler = ISOHandler.VIDE;
                context.VideoList.Add(new MP4VideoDescription());

                var reader = new BByteReader(ptr, smallVisual.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = VisualSampleEntry.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var video = ref context.LastVideo;

                Assert.AreEqual(1, video.ReferenceIndex, "ReferenceIndex");
                Assert.AreEqual(0x61766331u, video.CodecTag, "CodecTag");

                Assert.AreEqual(560, video.Width, "Width");
                Assert.AreEqual(320, video.Height, "Height");
                Assert.AreEqual(24, video.Depth, "Depth");

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        static readonly byte[] smallVisual = {
	        // Offset 0x0005CE19 to 0x0005CE6E
	        0x00, 0x00, 0x00, 0x9B, 0x61, 0x76, 0x63, 0x31, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x30, 0x01, 0x40,
            0x00, 0x48, 0x00, 0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x0E, 0x4A, 0x56, 0x54, 0x2F, 0x41, 0x56, 0x43, 0x20, 0x43,
            0x6F, 0x64, 0x69, 0x6E, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18,
            0xFF, 0xFF
        };
    }
}
