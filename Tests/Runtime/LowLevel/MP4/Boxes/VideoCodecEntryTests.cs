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
    public class VideoCodecEntryTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidVideo_AllValueAreEqual()
        {
            fixed (byte* ptr = smallAVCC)
            {
                context.LastTrack.Handler = ISOHandler.VIDE;
                context.TrackList.Add(new MP4TrackContext());

                var reader = new BByteReader(ptr, smallAVCC.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = VideoCodecEntry.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(MediaCodec.H264, track.Codec, "Codec");

                Assert.AreEqual(51, track.CodecExtra.Length, "CodecExtra.Length");
                for (int i = 0; i < track.CodecExtra.Length; i++)
                    Assert.AreEqual(ptr[i], ((byte*)track.CodecExtra.Ptr)[i], $"CodecExtra.Ptr[{i}]");
                Assert.AreEqual(context.Allocator, track.CodecExtra.Allocator, "CodecExtra.Allocator");

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        static readonly byte[] smallAVCC = {
	        // small.mp4
	        0x00, 0x00, 0x00, 0x33, 0x61, 0x76, 0x63, 0x43, 0x01, 0x42, 0xC0, 0x1E, 
            0xFF, 0xE1, 0x00, 0x1B, 0x67, 0x42, 0xC0, 0x1E, 0x9E, 0x21, 0x81, 0x18, 
            0x53, 0x4D, 0x40, 0x40, 0x40, 0x50, 0x00, 0x00, 0x03, 0x00, 0x10, 0x00, 
            0x00, 0x03, 0x03, 0xC8, 0xF1, 0x62, 0xEE, 0x01, 0x00, 0x05, 0x68, 0xCE, 
            0x06, 0xCB, 0x20
        };
    }
}
