using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.TestTools;

namespace MP4.Boxes
{
    public class BoxTestCore
    {
        protected MP4JobContext context;

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            context = new MP4JobContext();
            context.Logger = new JobLogger(16, Allocator.Temp);
            context.Tracks = new UnsafeList<TRAKBox>(1, Allocator.Temp);
        }

        [OneTimeTearDown]
        protected virtual void OneTimeTearDown()
        {
            context.Logger.Dispose();
            context.Tracks.Dispose();
        }

        [SetUp]
        protected virtual void SetUp()
        {
            context.Tracks.Add(new TRAKBox());
        }

        [TearDown]
        protected virtual void TearDown()
        {
            context.Logger.Clear();
            context.Tracks.Clear();

            var newContext = new MP4JobContext();
            newContext.Logger = context.Logger;
            newContext.Tracks = context.Tracks;

            context = newContext;
        }
    }
}
