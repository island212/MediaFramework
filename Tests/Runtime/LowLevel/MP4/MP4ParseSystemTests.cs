using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
            system.OnDestroy();
        }

        public class Printer
        {
            public System.Action Action;
            public Printer(System.Action action) => Action = action;
            ~Printer() => Action?.Invoke();
        }

        [Test]
        public unsafe void Open_ValidFile_NoException()
        {
            var handle = system.Open(k_VideoPath);

            Assert.AreNotEqual(MediaHandle.Invalid, handle, "MediaHandle");


            ref readonly var logger = ref system.GetLogs(handle);
            var printer = new Printer(logger.PrintAll);

            ref readonly var avio = ref system.GetAVIOContext(handle);

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
