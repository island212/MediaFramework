﻿using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;

public class TKHDBoxTests
{
    readonly byte[] tkhdSmallVersion0 = {
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

    [Test]
    public unsafe void Read_ValidTKHD_AllValueAreEqual()
    {
        fixed (byte* ptr = tkhdSmallVersion0)
        {
            using var validator = new MP4Validator(Validation.Strict, Allocator.Temp);

            var context = new MP4JobContext();
            context.Reader = new BByteReader(ptr, tkhdSmallVersion0.Length);
            context.Validator = validator;

            var tkhd = new TKHDBox();

            var isoBox = context.Reader.ReadISOBox();
            var error = TKHDBox.Read(ref context, ref tkhd, isoBox);

            Assert.AreEqual(MP4Error.None, error, "Error");
            Assert.IsTrue(!validator.HasError, "Validator.HasError");

            Assert.AreEqual(1, tkhd.TrackID, "TrackID");
            Assert.AreEqual(498000, tkhd.Duration, "Duration");
        }
    }
}