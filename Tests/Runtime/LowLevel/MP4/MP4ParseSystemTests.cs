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
            system.OnCreate();
        }

        [TearDown]
        public void TearDown()
        {
            system.OnDestroy();
        }

        [Test]
        public unsafe void Open_ValidFile_NoException()
        {
            var handle = system.Open(k_VideoPath);

            Assert.AreNotEqual(MediaHandle.Invalid, handle, "MediaHandle");

            ref var media = ref MP4ParseSystem.AsRef(handle);

            Assert.IsTrue(media.File.IsValid(), "MOOV.Offset");
            Assert.AreEqual(FileStatus.Open, media.File.Status, "File.Status");
            Assert.AreEqual(k_VideoFileSize, media.FileSize, "FileSize");
        }

        [Test]
        public unsafe void Open_AbsentFile_InvalidHandle()
        {
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex($"^{MP4Error.FileNotFound}"));

            var handle = system.Open("File.mp4");

            Assert.AreEqual(MediaHandle.Invalid, handle);
        }

        [Test]
        public unsafe void Prepare_ValidFile_NoException()
        {
            var handle = system.Open(k_VideoPath);

            Assert.AreNotEqual(MediaHandle.Invalid, handle, "MediaHandle");

            ref var media = ref MP4ParseSystem.AsRef(handle);

            Assert.IsTrue(media.File.IsValid(), "MOOV.Offset");
            Assert.AreEqual(FileStatus.Open, media.File.Status, "File.Status");
            Assert.AreEqual(k_VideoFileSize, media.FileSize, "FileSize");

            try
            {
                system.Prepare(handle).Complete();

                Assert.AreEqual(k_MoovOffset, media.MOOV.Offset, "MOOV.Offset");
                Assert.AreEqual(k_MoovLength, media.MOOV.Length, "MOOV.Length");

                Assert.AreEqual(k_MdatOffset, media.MDAT.Offset, "MDAT.Offset");
                Assert.AreEqual(k_MdatLength, media.MDAT.Length, "MDAT.Length");
            }
            finally
            {
                media.Logger.PrintAll();
            }
        }
    }
}
