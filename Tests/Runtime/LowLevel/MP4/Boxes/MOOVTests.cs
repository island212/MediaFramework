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
        protected override void SetUp() { }

        [Test]
        public unsafe void Read_Valid2Track_AllValueAreEqual()
        {
            fixed (byte* ptr = MOOVBuffer)
            {
                var reader = new BByteReader(ptr, MOOVBuffer.Length, Allocator.None);

                var moovBox = reader.ReadISOBox();

               var error = ISOBMFF.Read(ref context, ref reader, ref logger, moovBox);

                PrintLog();

                Assert.AreEqual(MP4Error.None, error, "Error");
                Assert.AreEqual(0, logger.Length, "Logger.Length");

                //MVHD
                Assert.AreEqual(501120, context.Duration, "MVHD.Duration");
                Assert.AreEqual(90000, context.Timescale, "MVHD.Timescale");
                Assert.AreEqual(3, context.NextTrackID, "MVHD.NextTrackID");

                //TRAK
                Assert.AreEqual(2, context.TrackList.Length, "Tracks.Length");

                ref var track1 = ref context.TrackList.ElementAt(0);

                Assert.AreEqual(ISOHandler.VIDE, track1.Handler, "track1.Handler"); 
                Assert.AreEqual(1, track1.TrackID, "track1.TKHD.TrackID");     
                Assert.AreEqual(498000, track1.Duration, "track1.MDHD.Duration");
                Assert.AreEqual(90000, track1.Timescale, "track1.MDHD.Timescale");
                Assert.AreEqual(ISOLanguage.UND, track1.Language, "track1.MDHD.Language");
                //Assert.AreEqual(385, track1.STSDIndex, "track1.STSD.Index");
                throw new System.NotImplementedException();
                Assert.AreEqual(1, track1.STTS.EntryCount, "track1.STTS.EntryCount");
                Assert.AreEqual(572, track1.STTS.SampleIndex, "track1.STTS.SampleIndex");
                Assert.AreEqual(2, track1.STSC.EntryCount, "track1.STSC.EntryCount");
                Assert.AreEqual(1280, track1.STSC.SampleIndex, "track1.STSC.SampleIndex");
                Assert.AreEqual(42, track1.STCO.EntryCount, "track1.STCO.EntryCount");
                Assert.AreEqual(1320, track1.STCO.SampleIndex, "track1.STCO.SampleIndex");

                ref var track2 = ref context.TrackList.ElementAt(1);

                Assert.AreEqual(ISOHandler.SOUN, track2.Handler, "track2.Handler");
                Assert.AreEqual(2, track2.TrackID, "track2.TKHD.TrackID");
                Assert.AreEqual(267264, track2.Duration, "track2.MDHD.Duration");
                Assert.AreEqual(48000, track2.Timescale, "track2.MDHD.Timescale");
                Assert.AreEqual(ISOLanguage.ENG, track2.Language, "track2.MDHD.Language");
                //Assert.AreEqual(1927, track2.STSDIndex, "track2.STSD.Index");
                throw new System.NotImplementedException();
                Assert.AreEqual(1, track2.STTS.EntryCount, "track2.STTS.EntryCount");
                Assert.AreEqual(2046, track2.STTS.SampleIndex, "track2.STTS.SampleIndex");
                Assert.AreEqual(2, track2.STSC.EntryCount, "track2.STSC.EntryCount");
                Assert.AreEqual(3134, track2.STSC.SampleIndex, "track2.STSC.SampleIndex");
                Assert.AreEqual(38, track2.STCO.EntryCount, "track2.STCO.EntryCount");
                Assert.AreEqual(3174, track2.STCO.SampleIndex, "track2.STCO.SampleIndex");
            }
        }
    }
}
