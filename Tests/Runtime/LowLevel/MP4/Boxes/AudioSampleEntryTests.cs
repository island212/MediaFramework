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
    public class AudioSampleEntryTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidAudio_AllValueAreEqual()
        {
            fixed (byte* ptr = audioSampleEntry)
            {
                context.LastTrack.Handler = ISOHandler.SOUN;

                var reader = new BByteReader(ptr, audioSampleEntry.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = AudioSampleEntry.Read(ref context, ref reader, ref logger, isoBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(1, track.ReferenceIndex, "ReferenceIndex");
                Assert.AreEqual(0x6d703461u, track.CodecTag, "CodecTag");

                Assert.AreEqual(1, track.ChannelCount, "ChannelCount");
                Assert.AreEqual(16, track.SampleSize, "SampleSize");
                Assert.AreEqual(48000, track.SampleRate, "SampleRate");

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        static readonly byte[] audioSampleEntry = {
	        // Offset 0x0005D41F to 0x0005D442 small.mp4
	        0x00, 0x00, 0x00, 0x57, 0x6D, 0x70, 0x34, 0x61, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x80, 0x00, 0x00
        };
    }
}
