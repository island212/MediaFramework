using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using MP4.Boxes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace MP4
{
    public class MP4BlobJobTests
    {
        NativeReference<JobLogger> Logger;
        NativeReference<BlobAssetReference<MP4Header>> Header;

        [SetUp]
        public void SetUp()
        {
            Logger = new NativeReference<JobLogger>(Allocator.TempJob);
            Header = new NativeReference<BlobAssetReference<MP4Header>>(Allocator.TempJob);

            Logger.Value = new JobLogger(16, Allocator.TempJob);
        }

        [TearDown]
        public void TearDown()
        {
            Logger.Value.Dispose();
            Logger.Dispose();

            Header.Value.Dispose();
            Header.Dispose();
        }


        [Test]
        public unsafe void Read_Valid2Track_AllValueAreEqual()
        {
            fixed (byte* ptr = BoxTestCore.MOOVBuffer)
            {
                new MP4BlobJob {
                    Reader = new BByteReader(ptr, BoxTestCore.MOOVBuffer.Length),
                    Logger = Logger,
                    Header = Header
                }.Run();

                ref var logger = ref Logger.AsRef();
                for (int i = 0; i < logger.Length; i++)
                {
                    UnityEngine.Debug.LogError(logger.MessageAt(i));
                }

                ref var header = ref Header.AsRef();

                Assert.AreEqual(0, logger.Length, "Logger.Length");

                Assert.AreEqual(501120, header.Value.Duration, "Duration");
                Assert.AreEqual(90000, header.Value.Timescale, "Timescale");

                Assert.AreEqual(1, header.Value.Videos.Length, "Videos.Length");

                ref var video1 = ref header.Value.Videos[0];

                Assert.AreEqual(1, video1.ID, "video1.ID");
                Assert.AreEqual(498000, video1.Duration, "video1.Duration");
                Assert.AreEqual(90000, video1.Timescale, "video1.Timescale");
                Assert.AreEqual(ISOLanguage.UND, video1.Language, "video1.Language");

                Assert.AreEqual(videoSTTS.Length, video1.TimeToSampleTable.Length, "video1.TimeToSampleTable.Length");
                for (int i = 0; i < videoSTTS.Length; i++)
                {
                    Assert.AreEqual(videoSTTS[i].count, video1.TimeToSampleTable[i].count, $"video1.TimeToSampleTable[{i}].count");
                    Assert.AreEqual(videoSTTS[i].delta, video1.TimeToSampleTable[i].delta, $"video1.TimeToSampleTable[{i}].delta");
                }

                Assert.AreEqual(videoSTSC.Length, video1.SampleToChunkTable.Length, "video1.SampleToChunkTable.Length");
                for (int i = 0; i < videoSTSC.Length; i++)
                {
                    Assert.AreEqual(videoSTSC[i].firstChunk, video1.SampleToChunkTable[i].firstChunk, $"video1.SampleToChunkTable[{i}].firstChunk");
                    Assert.AreEqual(videoSTSC[i].samplesPerChunk, video1.SampleToChunkTable[i].samplesPerChunk, $"video1.SampleToChunkTable[{i}].samplesPerChunk");
                    Assert.AreEqual(videoSTSC[i].sampleDescriptionIndex, video1.SampleToChunkTable[i].sampleDescriptionIndex, $"video1.SampleToChunkTable[{i}].sampleDescriptionIndex");
                }

                Assert.AreEqual(videoSTCO.Length, video1.ChunkOffsetTable.Length, "video1.TimeToSampleTable.Length");
                for (int i = 0; i < videoSTCO.Length; i++)
                {
                    Assert.AreEqual(videoSTCO[i], video1.ChunkOffsetTable[i].value, $"video1.ChunkOffsetTable[{i}].value");
                }

                Assert.AreEqual(1, header.Value.Audios.Length, "Audios.Length");

                ref var audio1 = ref header.Value.Audios[0];

                Assert.AreEqual(2, audio1.ID, "audio1.ID");
                Assert.AreEqual(267264, audio1.Duration, "audio1.Duration");
                Assert.AreEqual(48000, audio1.Timescale, "audio1.Timescale");
                Assert.AreEqual(ISOLanguage.ENG, audio1.Language, "audio1.Language");

                Assert.AreEqual(audioSTTS.Length, audio1.TimeToSampleTable.Length, "audio1.TimeToSampleTable.Length");
                for (int i = 0; i < audioSTTS.Length; i++)
                {
                    Assert.AreEqual(audioSTTS[i].count, audio1.TimeToSampleTable[i].count, $"audio1.TimeToSampleTable[{i}].count");
                    Assert.AreEqual(audioSTTS[i].delta, audio1.TimeToSampleTable[i].delta, $"audio1.TimeToSampleTable[{i}].delta");
                }

                Assert.AreEqual(audioSTSC.Length, audio1.SampleToChunkTable.Length, "audio1.SampleToChunkTable.Length");
                for (int i = 0; i < audioSTSC.Length; i++)
                {
                    Assert.AreEqual(audioSTSC[i].firstChunk, audio1.SampleToChunkTable[i].firstChunk, $"audio1.SampleToChunkTable[{i}].firstChunk");
                    Assert.AreEqual(audioSTSC[i].samplesPerChunk, audio1.SampleToChunkTable[i].samplesPerChunk, $"audio1.SampleToChunkTable[{i}].samplesPerChunk");
                    Assert.AreEqual(audioSTSC[i].sampleDescriptionIndex, audio1.SampleToChunkTable[i].sampleDescriptionIndex, $"audio1.SampleToChunkTable[{i}].sampleDescriptionIndex");
                }

                Assert.AreEqual(audioSTCO.Length, audio1.ChunkOffsetTable.Length, "audio1.TimeToSampleTable.Length");
                for (int i = 0; i < audioSTCO.Length; i++)
                {
                    Assert.AreEqual(audioSTCO[i], audio1.ChunkOffsetTable[i].value, $"audio1.ChunkOffsetTable[{i}].value");
                }
            }
        }

        public static readonly TimeSample[] videoSTTS = new TimeSample[]
        {
            new TimeSample { count = 166, delta = 3000 }
        };

        public static readonly SampleChunk[] videoSTSC = new SampleChunk[]
        {
            new SampleChunk { firstChunk = 1, samplesPerChunk = 4, sampleDescriptionIndex = 1 },
            new SampleChunk { firstChunk = 42, samplesPerChunk = 2, sampleDescriptionIndex = 1 }
        };

        public static readonly uint[] videoSTCO = new uint[]
        {
            168,29670,35828,41992,46948,51365,55256,58533,60511,63583,66625,73667,86405,99454,117926,132032,
            146514,157301,166428,177318,190310,205569,220588,237207,256612,271882,278121,281855,287705,294467,
            301211,306601,311031,317332,322924,331323,339401,350704,359298,365867,373722,378471
        };

        public static readonly TimeSample[] audioSTTS = new TimeSample[]
        {
            new TimeSample { count = 261, delta = 1024 }
        };

        public static readonly SampleChunk[] audioSTSC = new SampleChunk[]
        {
            new SampleChunk { firstChunk = 1, samplesPerChunk = 7, sampleDescriptionIndex = 1 },
            new SampleChunk { firstChunk = 38, samplesPerChunk = 2, sampleDescriptionIndex = 1 }
        };

        public static readonly uint[] audioSTCO = new uint[]
        {
            28045,34203,40420,45345,49831,53902,57230,62292,65256,72197,84679,97832,116129,130185,144732,
            164714,175714,188846,204141,219140,235853,255153,270745,276839,286150,292888,299626,304999,309374,
            315500,321292,329670,349020,357564,364185,372249,377067,379719
        };
    }
}
