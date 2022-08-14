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

namespace MP4.Boxes
{
    public class STTSTests : BoxTestCore
    {
        [Test]
        public unsafe void Read_Valid_AllValueAreEqual()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                var reader = new BByteReader(ptr, sttsSmallVideo.Length);

                var isoBox = reader.ReadISOBox();
                var error = STTSBox.Read(ref context, ref reader, isoBox);

                for (int i = 0; i < context.Logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

                ref var track = ref context.CurrentTrack;

                Assert.AreEqual(1, track.STTS.EntryCount, "EntryCount");
                Assert.AreEqual(16, track.STTS.SampleIndex, "SampleIndex");
                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }

        [Test]
        public unsafe void Read_OneNoneDefaultValue_DuplicateBox()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.CurrentTrack;
                track.STTS.EntryCount = 1;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length);

                var isoBox = reader.ReadISOBox();
                var error = STTSBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_InvalidBoxSize()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.size = 0;

                var error = STTSBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneSample_InvalidEntryCount()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.size += STTSBox.SampleSize;

                var error = STTSBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidEntryCount, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_InvalidSizeByOneByte_InvalidBoxSize()
        {
            fixed (byte* ptr = sttsSmallVideo)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, sttsSmallVideo.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.size = STTSBox.MinSize + 1;

                var error = STTSBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        readonly byte[] sttsSmallVideo = {
	        // Offset 0x0005CEB4 to 0x0005CECB small.mp4
	        0x00, 0x00, 0x00, 0x18, 0x73, 0x74, 0x74, 0x73, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xA6, 0x00, 0x00, 0x0B, 0xB8
        };
    }
}