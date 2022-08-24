﻿using MediaFramework.LowLevel.MP4;
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
    public class STSDTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_ValidVideo_AllValueAreEqual()
        {
            fixed (byte* ptr = stsdSmallVideo)
            {
                context.LastTrack.Handler = ISOHandler.VIDE;

                var reader = new BByteReader(ptr, stsdSmallVideo.Length, Allocator.None);

                var isoBox = reader.ReadISOBox();
                var error = STSDBox.Read(ref context, ref reader, ref logger, isoBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                ref var track = ref context.LastTrack;

                Assert.AreEqual(0, track.Descriptions.Offset, "Offset");
                Assert.AreEqual(1, track.Descriptions.Length, "Length");

                ref var video = ref context.LastVideo;

                Assert.AreEqual(1, video.ReferenceIndex, "ReferenceIndex");

                Assert.AreEqual(VideoCodec.H264, video.CodecID, "CodecID");
                Assert.AreEqual(0x61766331u, video.CodecTag, "CodecTag");

                Assert.AreEqual(66, video.Profile.Type, "Type");
                Assert.AreEqual(192, video.Profile.Constraints, "Constraints");
                Assert.AreEqual(30, video.Profile.Level, "Level");

                Assert.AreEqual(560, video.Width, "Width");
                Assert.AreEqual(320, video.Height, "Height");
                Assert.AreEqual(24, video.Depth, "Depth");

                Assert.AreEqual(115, video.SPS.Offset, "SPS.Offset");
                Assert.AreEqual(30, video.SPS.Length, "SPS.Length");

                Assert.AreEqual(145, video.PPS.Offset, "PPS.Offset");
                Assert.AreEqual(8, video.PPS.Length, "PPS.Length");

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        //[Test]
        //public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        //{
        //    fixed (byte* ptr = stsdSmallVideo)
        //    {
        //        ref var track = ref context.LastTrack;
        //        track.STSDIndex = 5;

        //        var reader = new BByteReader(ptr, stsdSmallVideo.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        var error = STSDBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
        //        Assert.AreEqual(1, logger.Length, "Logger.Length");
        //    }
        //}

        static readonly byte[] stsdSmallVideo = {
	        // Offset 0x0005CE09 to 0x0005CEB3 small.mp4
	        0x00, 0x00, 0x00, 0xAB, 0x73, 0x74, 0x73, 0x64, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x9B, 0x61, 0x76, 0x63, 0x31,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x30, 0x01, 0x40, 0x00, 0x48, 0x00, 0x00, 0x00, 0x48, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x0E, 0x4A, 0x56, 0x54, 0x2F, 0x41,
            0x56, 0x43, 0x20, 0x43, 0x6F, 0x64, 0x69, 0x6E, 0x67, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x18, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x61, 0x76,
            0x63, 0x43, 0x01, 0x42, 0xC0, 0x1E, 0xFF, 0xE1, 0x00, 0x1B, 0x67, 0x42,
            0xC0, 0x1E, 0x9E, 0x21, 0x81, 0x18, 0x53, 0x4D, 0x40, 0x40, 0x40, 0x50,
            0x00, 0x00, 0x03, 0x00, 0x10, 0x00, 0x00, 0x03, 0x03, 0xC8, 0xF1, 0x62,
            0xEE, 0x01, 0x00, 0x05, 0x68, 0xCE, 0x06, 0xCB, 0x20, 0x00, 0x00, 0x00,
            0x12, 0x63, 0x6F, 0x6C, 0x72, 0x6E, 0x63, 0x6C, 0x63, 0x00, 0x01, 0x00,
            0x01, 0x00, 0x01
        };
    }
}
