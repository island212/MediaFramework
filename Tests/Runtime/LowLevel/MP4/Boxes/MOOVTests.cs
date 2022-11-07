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
    public class MOOVTests : BoxTestCore
    {
        protected override void SetUp()
        {
            context = new MP4Context(Allocator.Temp);
            logger = new JobLogger(16, Allocator.Temp);
        }

        [Test]
        public unsafe void Read_Valid2Track_AllValueAreEqual()
        {
            fixed (byte* ptr = MOOVBuffer)
            {
                var reader = new BByteReader(ptr, MOOVBuffer.Length, Allocator.None);

                var moovBox = reader.ReadISOBox();

               var error = ISOBMFF.Read(ref context, ref reader, ref logger, moovBox);

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                Assert.AreEqual(501120, context.Duration, "context.Duration");
                Assert.AreEqual(90000, context.Timescale, "context.Timescale");
                Assert.AreEqual(2, context.TrackList.Length, "Tracks.Length");

                ref var track1 = ref context.TrackList.ElementAt(0);

                Assert.AreEqual(ISOHandler.VIDE, track1.Handler, "track1.Handler"); 
                Assert.AreEqual(1, track1.TrackID, "track1.TrackID");   
                Assert.AreEqual(498000, track1.Duration, "track1.Duration");
                Assert.AreEqual(90000, track1.Timescale, "track1.Timescale");
                Assert.AreEqual(ISOLanguage.UND, track1.Language, "track1.Language");

                Assert.AreEqual(1, track1.STTS.Length, "STTS.Length");
                Assert.AreEqual(2, track1.STSC.Length, "STSC.Length");
                Assert.AreEqual(42, track1.STCO.Length, "STCO.Length");

                ref var track2 = ref context.TrackList.ElementAt(1);

                Assert.AreEqual(ISOHandler.SOUN, track2.Handler, "track2.Handler");
                Assert.AreEqual(2, track2.TrackID, "track2.TrackID");
                Assert.AreEqual(267264, track2.Duration, "track2.Duration");
                Assert.AreEqual(48000, track2.Timescale, "track2.Timescale");
                Assert.AreEqual(ISOLanguage.ENG, track2.Language, "track2.Language");

                Assert.AreEqual(1, track2.STTS.Length, "track2.STTS.Length");
                Assert.AreEqual(2, track2.STSC.Length, "track2.STSC.Length");
                Assert.AreEqual(38, track2.STCO.Length, "track2.STCO.Length");

                Assert.AreEqual(0, reader.Remains, "Remains");
            }
        }
    }
}
