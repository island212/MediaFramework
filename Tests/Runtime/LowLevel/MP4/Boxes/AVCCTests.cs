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
using MediaFramework.LowLevel.Codecs;

namespace MP4.Boxes
{
    public class AVCCTests : BoxTestCore
    {
        //protected override void SetUp()
        //{
        //    base.SetUp();

        //    context.VideoList.Add(new MP4VideoDescription());
        //}

        //[Test]
        //public unsafe void Read_Normal_AllValueAreEqual()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        PrintLog();

        //        Assert.AreEqual(MP4Error.None, error, "Error");
        //        Assert.AreEqual(0, logger.Errors, "Logger.Errors");

        //        ref var video = ref context.LastVideo;

        //        Assert.AreEqual(VideoCodec.H264, video.CodecID, "CodecID");

        //        //Assert.AreEqual(66, video.Profile.Type, "Type");
        //        //Assert.AreEqual(192, video.Profile.Constraints, "Constraints");
        //        //Assert.AreEqual(30, video.Profile.Level, "Level");

        //        //Assert.AreEqual(13, video.SPS.Offset, "SPS.Offset");
        //        //Assert.AreEqual(30, video.SPS.Length, "SPS.Length");

        //        //Assert.AreEqual(43, video.PPS.Offset, "PPS.Offset");
        //        //Assert.AreEqual(8, video.PPS.Length, "PPS.Length");

        //        Assert.AreEqual(0, reader.Remains, "Remains");
        //    }
        //}

        //[Test]
        //public unsafe void Read_WithExt_AllValueAreEqual()
        //{
        //    fixed (byte* ptr = avccTeaser)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        PrintLog();

        //        Assert.AreEqual(MP4Error.None, error, "Error");
        //        Assert.AreEqual(0, logger.Errors, "Logger.Errors");

        //        ref var video = ref context.LastVideo;

        //        Assert.AreEqual(VideoCodec.H264, video.CodecID, "CodecID");

        //        //Assert.AreEqual(100, video.Profile.Type, "Type");
        //        //Assert.AreEqual(0, video.Profile.Constraints, "Constraints");
        //        //Assert.AreEqual(42, video.Profile.Level, "Level");

        //        //Assert.AreEqual(13, video.SPS.Offset, "SPS.Offset");
        //        //Assert.AreEqual(25, video.SPS.Length, "SPS.Length");

        //        //Assert.AreEqual(38, video.PPS.Offset, "PPS.Offset");
        //        //Assert.AreEqual(7, video.PPS.Length, "PPS.Length");

        //        Assert.AreEqual(0, reader.Remains, "Remains");
        //    }
        //}

        //[Test]
        //public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        ref var video = ref context.LastVideo;

        //        //video.Profile.Type = 33;

        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(2, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //[Test]
        //public unsafe void Read_BoxSize13_InvalidBoxSize()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        isoBox.size = 13;

        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(1, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //[Test]
        //public unsafe void Read_BoxSize14_InvalidBoxSize()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        isoBox.size = 14;

        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(3, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //[Test]
        //public unsafe void Read_BoxSize16_InvalidBoxSize()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        isoBox.size = 16;

        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(4, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //[Test]
        //public unsafe void Read_BoxSize43_InvalidBoxSize()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        isoBox.size = 43;

        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(5, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //[Test]
        //public unsafe void Read_BoxSize44_InvalidBoxSize()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        isoBox.size = 44;

        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(6, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //[Test]
        //public unsafe void Read_BoxSize46_InvalidBoxSize()
        //{
        //    fixed (byte* ptr = avccSmall)
        //    {
        //        var reader = new BByteReader(ptr, avccSmall.Length, Allocator.None);

        //        var isoBox = reader.ReadISOBox();
        //        isoBox.size = 46;

        //        var error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox);

        //        Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
        //        Assert.AreEqual(1, logger.Errors, "Logger.Errors");
        //        Assert.AreEqual(7, logger.First().tag % 100, "Log.Tag");
        //    }
        //}

        //static readonly byte[] avccSmall = {
	       // // Offset 0x0005CE6F to 0x0005CEA1 small.mp4
	       // 0x00, 0x00, 0x00, 0x33, 0x61, 0x76, 0x63, 0x43, 0x01, 0x42, 0xC0, 0x1E,
        //    0xFF, 0xE1, 0x00, 0x1B, 0x67, 0x42, 0xC0, 0x1E, 0x9E, 0x21, 0x81, 0x18,
        //    0x53, 0x4D, 0x40, 0x40, 0x40, 0x50, 0x00, 0x00, 0x03, 0x00, 0x10, 0x00,
        //    0x00, 0x03, 0x03, 0xC8, 0xF1, 0x62, 0xEE, 0x01, 0x00, 0x05, 0x68, 0xCE,
        //    0x06, 0xCB, 0x20
        //};

        //static readonly byte[] avccTeaser = {
	       // // Offset 0x0000025D to 0x0000028C
	       // 0x00, 0x00, 0x00, 0x31, 0x61, 0x76, 0x63, 0x43, 0x01, 0x64, 0x00, 0x2A,
        //    0xFF, 0xE1, 0x00, 0x16, 0x67, 0x64, 0x00, 0x2A, 0xAC, 0x2B, 0x20, 0x0F,
        //    0x00, 0x44, 0xFC, 0xB8, 0x08, 0x80, 0x00, 0x01, 0xF4, 0x80, 0x00, 0x5D,
        //    0xC0, 0x42, 0x01, 0x00, 0x04, 0x68, 0xEB, 0x8F, 0x2C, 0xFD, 0xF8, 0xF8,
        //    0x00
        //};
    }
}
