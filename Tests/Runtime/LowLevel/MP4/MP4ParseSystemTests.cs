using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.TestTools;

namespace MP4
{
    public class MP4ParseSystemTests
    {
        const int k_VideoFileSize = 113169;
        const int k_MoovOffset = 107525, k_MoovLength = 5644;
        const int k_MdatOffset = 40, k_MdatLength = 107485;

        const string k_VideoPath = "Assets/MediaFramework/Tests/Media/video_audio.mp4";

        MP4ParseSystem system;

        [SetUp]
        public void SetUp()
        {
            system.OnCreate(1);
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                system.PrintAll();
            }
            system.OnDestroy();
        }

        [Test]
        public unsafe void Open_ValidFile_NoException()
        {
            var handle = system.Open(k_VideoPath);

            Assert.AreNotEqual(MediaHandle.Invalid, handle, "MediaHandle");

            ref readonly var logger = ref system.GetLogs(handle);
            ref readonly var avio = ref system.GetAVIOContext(handle);

            Assert.AreEqual(0, logger.Errors, "Logger.Errors");
            Assert.IsTrue(avio.File.IsValid(), "File.IsValid");
            Assert.AreEqual(FileStatus.Open, avio.File.Status, "File.Status");
            Assert.AreEqual(k_VideoFileSize, avio.FileSize, "FileSize");
        }

        [Test]
        public unsafe void Open_AbsentFile_InvalidHandle()
        {
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex($"^FileNotFound"));

            var handle = system.Open("File.mp4");

            Assert.AreEqual(MediaHandle.Invalid, handle);
        }

        [Test]
        public unsafe void ParseMP4_ValidFile_NoException()
        {
            var handle = system.Open(k_VideoPath);

            Assert.AreNotEqual(MediaHandle.Invalid, handle, "MediaHandle");

            var job = system.ParseMP4(handle);

            job.Complete();

            ref readonly var logger = ref system.GetLogs(handle);
            Assert.AreEqual(0, logger.Errors, "Logger.Errors");

            ref readonly var avio = ref system.GetAVIOContext(handle);
            Assert.IsTrue(avio.File.IsValid(), "File.IsValid");
            Assert.AreEqual(FileStatus.Open, avio.File.Status, "FileStatus");
            Assert.AreEqual(k_VideoFileSize, avio.FileSize, "FileSize");
            Assert.AreEqual(0, avio.Commands.CommandCount, "CommandCount");

            ref readonly var mp4 = ref system.GetHeader(handle);
            Assert.AreEqual(Allocator.Persistent, mp4.Allocator, "Allocator");
            Assert.AreEqual(107525, mp4.MOOV.Offset, "MOOV.Offset");
            Assert.AreEqual(5644, mp4.MOOV.Length, "MOOV.Length");
            Assert.AreEqual(40, mp4.MDAT.Offset, "MDAT.Offset");
            Assert.AreEqual(107485, mp4.MDAT.Length, "MDAT.Length");

            Assert.AreEqual(3750600896, mp4.CreationTime.value, "CreationTime");
            Assert.AreEqual(5005, mp4.Duration, "Duration");
            Assert.AreEqual(1000, mp4.Timescale, "Timescale");

            Assert.AreEqual(2, mp4.TrackList.Length, "TrackList.Length");

            ref readonly var video = ref mp4.TrackList.ElementAt(0);
            Assert.AreEqual(ISOHandler.VIDE, video.Handler, "Video.Handler");
            Assert.AreEqual(1, video.TrackID, "Video.TrackID");
            Assert.AreEqual(150150, video.Duration, "Video.Duration");
            Assert.AreEqual(30000, video.Timescale, "Video.Timescale");
            Assert.AreEqual(MediaCodec.H264, video.Codec, "Video.Codec");
            Assert.AreEqual(0x61766331, video.CodecTag, "Video.CodecTag"); // avc1
            Assert.AreEqual(1, video.ReferenceIndex, "Video.ReferenceIndex");
            Assert.AreEqual(640, video.Width, "Video.Width");
            Assert.AreEqual(360, video.Height, "Video.Height");
            Assert.AreEqual(24, video.Depth, "Video.Depth");
            // TODO: Compare every byte
            Assert.AreEqual(51, video.CodecExtra.Length, "Video.CodecExtra.Length");
            Assert.AreEqual(0, video.SampleRate, "Video.SampleRate");
            Assert.AreEqual(0, video.ChannelCount, "Video.ChannelCount");
            // TODO: Compare sample array (STTS, STSC, STCO)

            ref readonly var audio = ref mp4.TrackList.ElementAt(1);
            Assert.AreEqual(ISOHandler.SOUN, audio.Handler, "Audio.Handler");
            Assert.AreEqual(2, audio.TrackID, "Audio.TrackID");
            Assert.AreEqual(221524, audio.Duration, "Audio.Duration");
            Assert.AreEqual(44100, audio.Timescale, "Audio.Timescale");
            //Need to implement ESDS for audio
            //Assert.AreEqual(MediaCodec.AAC, audio.Codec, "Audio.Codec");
            Assert.AreEqual(0x6d703461, audio.CodecTag, "Audio.CodecTag"); // mp4a
            Assert.AreEqual(1, audio.ReferenceIndex, "Audio.ReferenceIndex");
            Assert.AreEqual(0, audio.Width, "Audio.Width");
            Assert.AreEqual(0, audio.Height, "Audio.Height");
            Assert.AreEqual(0, audio.Depth, "Audio.Depth");
            Assert.AreEqual(0, audio.CodecExtra.Length, "Audio.CodecExtra.Length");
            Assert.AreEqual(44100, audio.SampleRate, "Audio.SampleRate");
            Assert.AreEqual(2, audio.ChannelCount, "Audio.ChannelCount");
            Assert.AreEqual(16, audio.SampleSize, "Audio.SampleSize");
            // TODO: Compare sample array (STTS, STSC, STCO)
        }

        //[Test]
        //public unsafe void Prepare_ValidFile_NoException()
        //{
        //    var handle = system.Open(k_VideoPath);

        //    Assert.AreNotEqual(MediaHandle.Invalid, handle, "MediaHandle");

        //    ref readonly var avio = ref system.GetAVIOContext(handle);

        //    Assert.IsTrue(avio.File.IsValid(), "MOOV.Offset");
        //    Assert.AreEqual(FileStatus.Open, avio.File.Status, "File.Status");
        //    Assert.AreEqual(k_VideoFileSize, avio.FileSize, "FileSize");

        //    try
        //    {
        //        system.Parse(handle).Complete();

        //        Assert.AreEqual(k_MoovOffset, media.MOOV.Offset, "MOOV.Offset");
        //        Assert.AreEqual(k_MoovLength, media.MOOV.Length, "MOOV.Length");

        //        Assert.AreEqual(k_MdatOffset, media.MDAT.Offset, "MDAT.Offset");
        //        Assert.AreEqual(k_MdatLength, media.MDAT.Length, "MDAT.Length");
        //    }
        //    finally
        //    {
        //        media.Logger.PrintAll();
        //    }
        //}
    }
}
