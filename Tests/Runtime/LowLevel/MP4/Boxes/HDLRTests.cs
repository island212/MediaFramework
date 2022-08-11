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
    internal class HDLRTests
    {
        readonly byte[] hdlrSmall = {
	        // Offset 0x0005CDA0 to 0x0005CDC0 small.mp4
	        0x00, 0x00, 0x00, 0x21, 0x68, 0x64, 0x6C, 0x72, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x76, 0x69, 0x64, 0x65, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private MP4JobContext context;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            context = new MP4JobContext();
            context.Logger = new JobLogger(16, Allocator.Temp);
            context.Tracks = new UnsafeList<TRAKBox>(1, Allocator.Temp);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            context.Logger.Dispose();
            context.Tracks.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            context.Tracks.Add(new TRAKBox());
        }

        [TearDown]
        public void TearDown()
        {
            var logger = context.Logger;
            var tracks = context.Tracks;

            logger.Clear();
            tracks.Clear();

            context = new MP4JobContext();
            context.Logger = logger;
            context.Tracks = tracks;
        }

        [Test]
        public unsafe void Read_VideoHandler_AllValueAreEqual()
        {
            fixed (byte* ptr = hdlrSmall)
            {
                var reader = new BByteReader(ptr, hdlrSmall.Length);

                var isoBox = reader.ReadISOBox();
                var error = HDLRBox.Read(ref context, ref reader, isoBox);

                for (int i = 0; i < context.Logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
                }

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

                ref var track = ref context.CurrentTrack;

                Assert.AreEqual(ISOHandler.VIDE, track.Handler, "Handler");
            }
        }

        [Test]
        public unsafe void Read_VideoHandlerWithName_AllValueAreEqual()
        {
            byte* ptr = stackalloc byte[hdlrSmall.Length + 7];
            for (int i = 0; i < hdlrSmall.Length; i++)
                ptr[i] = hdlrSmall[i];

            var reader = new BByteReader(ptr, hdlrSmall.Length);

            var isoBox = reader.ReadISOBox();
            isoBox.Size = (uint)hdlrSmall.Length + 7;

            var error = HDLRBox.Read(ref context, ref reader, isoBox);

            for (int i = 0; i < context.Logger.Length; i++)
            {
                UnityEngine.Debug.LogError(context.Logger.MessageAt(i));
            }

            Assert.AreEqual(MP4Error.None, error, "Error");
            Assert.AreEqual(0, context.Logger.Length, "Logger.Length");

            ref var track = ref context.CurrentTrack;

            Assert.AreEqual(ISOHandler.VIDE, track.Handler, "Handler");
        }

        [Test]
        public unsafe void Read_Duplicate_ReturnErrorAndLog()
        {
            fixed (byte* ptr = hdlrSmall)
            {
                ref var track = ref context.CurrentTrack;
                track.Handler = ISOHandler.VIDE;

                var reader = new BByteReader(ptr, hdlrSmall.Length);

                var isoBox = reader.ReadISOBox();
                var error = HDLRBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.DuplicateBox, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }

        [Test]
        public unsafe void Read_BoxSize0_ReturnErrorAndLog()
        {
            fixed (byte* ptr = hdlrSmall)
            {
                ref var track = ref context.CurrentTrack;

                var reader = new BByteReader(ptr, hdlrSmall.Length);

                var isoBox = reader.ReadISOBox();
                isoBox.Size = 0;

                var error = TKHDBox.Read(ref context, ref reader, isoBox);

                Assert.AreEqual(MP4Error.InvalidBoxSize, error, "Error");
                Assert.AreEqual(1, context.Logger.Length, "Logger.Length");
            }
        }
    }
}
