﻿using MediaFramework.LowLevel;
using MediaFramework.LowLevel.MP4;
using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MP4.Boxes
{
    public class BoxTestCore
    {
        protected MP4Context context;
        protected JobLogger logger;

        //[OneTimeSetUp]
        //protected virtual void OneTimeSetUp()
        //{
        //    context = new MP4Context(Allocator.Temp);
        //    logger = new JobLogger(16, Allocator.Temp);
        //}

        //[OneTimeTearDown]
        //protected virtual void OneTimeTearDown()
        //{
        //    logger.Dispose();
        //    context.Dispose();
        //}

        [SetUp]
        protected virtual void SetUp()
        {
            context = new MP4Context(Allocator.Temp);
            logger = new JobLogger(16, Allocator.Temp);
            context.TrackList.Add(new MP4TrackContext());
        }

        [TearDown]
        protected virtual void TearDown()
        {
            logger.Dispose();
            context.Dispose();
        }

        protected void PrintLog()
        {
            for (int i = 0; i < logger.Length; i++)
            {
                switch (logger.LogTypeAt(i))
                {
                    case UnityEngine.LogType.Log:
                        UnityEngine.Debug.Log($"{logger.LogTagAt(i)} - {logger.MessageAt(i)}");
                        break;
                    case UnityEngine.LogType.Warning:
                        UnityEngine.Debug.LogWarning($"{logger.LogTagAt(i)} - {logger.MessageAt(i)}");
                        break;
                    case UnityEngine.LogType.Error:
                        UnityEngine.Debug.LogError($"{logger.LogTagAt(i)} - {logger.MessageAt(i)}");
                        break;
                }
            }
        }

        public readonly static int MOOVIndex = 380040;

        public readonly static byte[] MOOVBuffer = {
	        // Offset 0x0005CC88 to 0x0005DA0A small.mp4
	        0x00, 0x00, 0x0D, 0x83, 0x6D, 0x6F, 0x6F, 0x76, 0x00, 0x00, 0x00, 0x6C,
            0x6D, 0x76, 0x68, 0x64, 0x00, 0x00, 0x00, 0x00, 0xC7, 0xCA, 0xEE, 0xA7,
            0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x01, 0x5F, 0x90, 0x00, 0x07, 0xA5, 0x80,
            0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18,
            0x69, 0x6F, 0x64, 0x73, 0x00, 0x00, 0x00, 0x00, 0x10, 0x80, 0x80, 0x80,
            0x07, 0x00, 0x4F, 0xFF, 0xFF, 0x0F, 0x7F, 0xFF, 0x00, 0x00, 0x06, 0x0A,
            0x74, 0x72, 0x61, 0x6B, 0x00, 0x00, 0x00, 0x5C, 0x74, 0x6B, 0x68, 0x64,
            0x00, 0x00, 0x00, 0x01, 0xC7, 0xCA, 0xEE, 0xA7, 0xC7, 0xCA, 0xEE, 0xA8,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x99, 0x50,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x40, 0x00, 0x00, 0x00, 0x02, 0x30, 0x00, 0x00, 0x01, 0x40, 0x00, 0x00,
            0x00, 0x00, 0x05, 0xA6, 0x6D, 0x64, 0x69, 0x61, 0x00, 0x00, 0x00, 0x20,
            0x6D, 0x64, 0x68, 0x64, 0x00, 0x00, 0x00, 0x00, 0xC7, 0xCA, 0xEE, 0xA7,
            0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x01, 0x5F, 0x90, 0x00, 0x07, 0x99, 0x50,
            0x55, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x68, 0x64, 0x6C, 0x72,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x76, 0x69, 0x64, 0x65,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x05, 0x5D, 0x6D, 0x69, 0x6E, 0x66, 0x00, 0x00, 0x00,
            0x14, 0x76, 0x6D, 0x68, 0x64, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x64, 0x69, 0x6E,
            0x66, 0x00, 0x00, 0x00, 0x1C, 0x64, 0x72, 0x65, 0x66, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0C, 0x75, 0x72, 0x6C,
            0x20, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x05, 0x1D, 0x73, 0x74, 0x62,
            0x6C, 0x00, 0x00, 0x00, 0xAB, 0x73, 0x74, 0x73, 0x64, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x9B, 0x61, 0x76, 0x63,
            0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x02, 0x30, 0x01, 0x40, 0x00, 0x48, 0x00, 0x00, 0x00, 0x48, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x0E, 0x4A, 0x56, 0x54, 0x2F,
            0x41, 0x56, 0x43, 0x20, 0x43, 0x6F, 0x64, 0x69, 0x6E, 0x67, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x18, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x61,
            0x76, 0x63, 0x43, 0x01, 0x42, 0xC0, 0x1E, 0xFF, 0xE1, 0x00, 0x1B, 0x67,
            0x42, 0xC0, 0x1E, 0x9E, 0x21, 0x81, 0x18, 0x53, 0x4D, 0x40, 0x40, 0x40,
            0x50, 0x00, 0x00, 0x03, 0x00, 0x10, 0x00, 0x00, 0x03, 0x03, 0xC8, 0xF1,
            0x62, 0xEE, 0x01, 0x00, 0x05, 0x68, 0xCE, 0x06, 0xCB, 0x20, 0x00, 0x00,
            0x00, 0x12, 0x63, 0x6F, 0x6C, 0x72, 0x6E, 0x63, 0x6C, 0x63, 0x00, 0x01,
            0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x18, 0x73, 0x74, 0x74, 0x73,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xA6,
            0x00, 0x00, 0x0B, 0xB8, 0x00, 0x00, 0x02, 0xAC, 0x73, 0x74, 0x73, 0x7A,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA6,
            0x00, 0x00, 0x56, 0x27, 0x00, 0x00, 0x0B, 0x20, 0x00, 0x00, 0x05, 0xBC,
            0x00, 0x00, 0x05, 0xE2, 0x00, 0x00, 0x05, 0xC1, 0x00, 0x00, 0x04, 0x37,
            0x00, 0x00, 0x04, 0x07, 0x00, 0x00, 0x03, 0xB6, 0x00, 0x00, 0x06, 0x45,
            0x00, 0x00, 0x03, 0x73, 0x00, 0x00, 0x05, 0x12, 0x00, 0x00, 0x03, 0x26,
            0x00, 0x00, 0x02, 0xE9, 0x00, 0x00, 0x03, 0x7B, 0x00, 0x00, 0x03, 0x4A,
            0x00, 0x00, 0x03, 0x6B, 0x00, 0x00, 0x02, 0xB6, 0x00, 0x00, 0x03, 0x4C,
            0x00, 0x00, 0x02, 0x7A, 0x00, 0x00, 0x02, 0xC7, 0x00, 0x00, 0x02, 0x2E,
            0x00, 0x00, 0x03, 0x16, 0x00, 0x00, 0x02, 0x26, 0x00, 0x00, 0x02, 0x7F,
            0x00, 0x00, 0x01, 0xEC, 0x00, 0x00, 0x01, 0xEA, 0x00, 0x00, 0x01, 0xF5,
            0x00, 0x00, 0x01, 0xEB, 0x00, 0x00, 0x01, 0xFA, 0x00, 0x00, 0x01, 0xE7,
            0x00, 0x00, 0x01, 0xFC, 0x00, 0x00, 0x01, 0xDD, 0x00, 0x00, 0x01, 0xC6,
            0x00, 0x00, 0x01, 0xAE, 0x00, 0x00, 0x01, 0xC8, 0x00, 0x00, 0x01, 0xB9,
            0x00, 0x00, 0x01, 0x90, 0x00, 0x00, 0x01, 0x93, 0x00, 0x00, 0x01, 0x8C,
            0x00, 0x00, 0x01, 0xDA, 0x00, 0x00, 0x01, 0xC2, 0x00, 0x00, 0x05, 0xD0,
            0x00, 0x00, 0x07, 0xB8, 0x00, 0x00, 0x06, 0x7A, 0x00, 0x00, 0x09, 0xA9,
            0x00, 0x00, 0x0A, 0x2C, 0x00, 0x00, 0x0A, 0x7C, 0x00, 0x00, 0x0C, 0xB3,
            0x00, 0x00, 0x09, 0x8C, 0x00, 0x00, 0x09, 0x52, 0x00, 0x00, 0x0C, 0x04,
            0x00, 0x00, 0x0D, 0xC1, 0x00, 0x00, 0x0F, 0x74, 0x00, 0x00, 0x10, 0x48,
            0x00, 0x00, 0x11, 0x06, 0x00, 0x00, 0x10, 0x61, 0x00, 0x00, 0x0C, 0x63,
            0x00, 0x00, 0x0C, 0x31, 0x00, 0x00, 0x0B, 0x42, 0x00, 0x00, 0x0C, 0x0D,
            0x00, 0x00, 0x0F, 0x32, 0x00, 0x00, 0x0A, 0x7B, 0x00, 0x00, 0x0D, 0x0F,
            0x00, 0x00, 0x0A, 0xE0, 0x00, 0x00, 0x0A, 0x0E, 0x00, 0x00, 0x0B, 0x6B,
            0x00, 0x00, 0x08, 0x74, 0x00, 0x00, 0x0C, 0x36, 0x00, 0x00, 0x09, 0xE6,
            0x00, 0x00, 0x06, 0x8D, 0x00, 0x00, 0x04, 0xF8, 0x00, 0x00, 0x07, 0x8A,
            0x00, 0x00, 0x07, 0xC1, 0x00, 0x00, 0x09, 0xF3, 0x00, 0x00, 0x07, 0xC7,
            0x00, 0x00, 0x0A, 0xCB, 0x00, 0x00, 0x0A, 0xD2, 0x00, 0x00, 0x0B, 0x74,
            0x00, 0x00, 0x0C, 0x28, 0x00, 0x00, 0x0A, 0x9A, 0x00, 0x00, 0x0C, 0x60,
            0x00, 0x00, 0x0D, 0x6D, 0x00, 0x00, 0x0C, 0x3E, 0x00, 0x00, 0x0F, 0xFC,
            0x00, 0x00, 0x0E, 0x82, 0x00, 0x00, 0x0B, 0x79, 0x00, 0x00, 0x0D, 0xE4,
            0x00, 0x00, 0x0D, 0x24, 0x00, 0x00, 0x0A, 0x17, 0x00, 0x00, 0x11, 0xAA,
            0x00, 0x00, 0x12, 0x65, 0x00, 0x00, 0x0D, 0x7B, 0x00, 0x00, 0x12, 0xA0,
            0x00, 0x00, 0x13, 0xD8, 0x00, 0x00, 0x11, 0x49, 0x00, 0x00, 0x0E, 0x59,
            0x00, 0x00, 0x10, 0x15, 0x00, 0x00, 0x16, 0x81, 0x00, 0x00, 0x09, 0xB4,
            0x00, 0x00, 0x06, 0xEB, 0x00, 0x00, 0x05, 0xEF, 0x00, 0x00, 0x05, 0x8A,
            0x00, 0x00, 0x03, 0xD7, 0x00, 0x00, 0x04, 0x0D, 0x00, 0x00, 0x03, 0xBB,
            0x00, 0x00, 0x04, 0x6B, 0x00, 0x00, 0x03, 0x40, 0x00, 0x00, 0x03, 0x30,
            0x00, 0x00, 0x02, 0xDE, 0x00, 0x00, 0x03, 0xAE, 0x00, 0x00, 0x05, 0xCF,
            0x00, 0x00, 0x04, 0x6C, 0x00, 0x00, 0x05, 0x69, 0x00, 0x00, 0x05, 0x00,
            0x00, 0x00, 0x06, 0xA1, 0x00, 0x00, 0x03, 0x35, 0x00, 0x00, 0x04, 0x1A,
            0x00, 0x00, 0x03, 0xFA, 0x00, 0x00, 0x06, 0x3D, 0x00, 0x00, 0x05, 0xD6,
            0x00, 0x00, 0x04, 0x68, 0x00, 0x00, 0x02, 0xD6, 0x00, 0x00, 0x04, 0xB5,
            0x00, 0x00, 0x02, 0xD9, 0x00, 0x00, 0x02, 0x7F, 0x00, 0x00, 0x02, 0x4D,
            0x00, 0x00, 0x02, 0x7D, 0x00, 0x00, 0x03, 0x8C, 0x00, 0x00, 0x02, 0x06,
            0x00, 0x00, 0x02, 0x01, 0x00, 0x00, 0x07, 0x7F, 0x00, 0x00, 0x05, 0xEF,
            0x00, 0x00, 0x05, 0xB8, 0x00, 0x00, 0x04, 0x0A, 0x00, 0x00, 0x02, 0x99,
            0x00, 0x00, 0x03, 0x1D, 0x00, 0x00, 0x07, 0xC5, 0x00, 0x00, 0x05, 0xAC,
            0x00, 0x00, 0x04, 0x78, 0x00, 0x00, 0x08, 0x71, 0x00, 0x00, 0x08, 0x99,
            0x00, 0x00, 0x08, 0xE9, 0x00, 0x00, 0x08, 0x99, 0x00, 0x00, 0x05, 0x73,
            0x00, 0x00, 0x07, 0xC7, 0x00, 0x00, 0x08, 0x3D, 0x00, 0x00, 0x0B, 0x59,
            0x00, 0x00, 0x0A, 0x36, 0x00, 0x00, 0x06, 0xBA, 0x00, 0x00, 0x05, 0xF9,
            0x00, 0x00, 0x07, 0x2E, 0x00, 0x00, 0x06, 0xEB, 0x00, 0x00, 0x04, 0xC6,
            0x00, 0x00, 0x04, 0xBA, 0x00, 0x00, 0x05, 0x66, 0x00, 0x00, 0x04, 0x31,
            0x00, 0x00, 0x06, 0x8A, 0x00, 0x00, 0x06, 0xCF, 0x00, 0x00, 0x06, 0xFE,
            0x00, 0x00, 0x04, 0x97, 0x00, 0x00, 0x02, 0x43, 0x00, 0x00, 0x03, 0xE2,
            0x00, 0x00, 0x04, 0x06, 0x00, 0x00, 0x02, 0xE6, 0x00, 0x00, 0x02, 0x6B,
            0x00, 0x00, 0x02, 0x75, 0x00, 0x00, 0x00, 0x28, 0x73, 0x74, 0x73, 0x63,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2A,
            0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xB8,
            0x73, 0x74, 0x63, 0x6F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A,
            0x00, 0x00, 0x00, 0xA8, 0x00, 0x00, 0x73, 0xE6, 0x00, 0x00, 0x8B, 0xF4,
            0x00, 0x00, 0xA4, 0x08, 0x00, 0x00, 0xB7, 0x64, 0x00, 0x00, 0xC8, 0xA5,
            0x00, 0x00, 0xD7, 0xD8, 0x00, 0x00, 0xE4, 0xA5, 0x00, 0x00, 0xEC, 0x5F,
            0x00, 0x00, 0xF8, 0x5F, 0x00, 0x01, 0x04, 0x41, 0x00, 0x01, 0x1F, 0xC3,
            0x00, 0x01, 0x51, 0x85, 0x00, 0x01, 0x84, 0x7E, 0x00, 0x01, 0xCC, 0xA6,
            0x00, 0x02, 0x03, 0xC0, 0x00, 0x02, 0x3C, 0x52, 0x00, 0x02, 0x66, 0x75,
            0x00, 0x02, 0x8A, 0x1C, 0x00, 0x02, 0xB4, 0xA6, 0x00, 0x02, 0xE7, 0x66,
            0x00, 0x03, 0x23, 0x01, 0x00, 0x03, 0x5D, 0xAC, 0x00, 0x03, 0x9E, 0x97,
            0x00, 0x03, 0xEA, 0x64, 0x00, 0x04, 0x26, 0x0A, 0x00, 0x04, 0x3E, 0x69,
            0x00, 0x04, 0x4C, 0xFF, 0x00, 0x04, 0x63, 0xD9, 0x00, 0x04, 0x7E, 0x43,
            0x00, 0x04, 0x98, 0x9B, 0x00, 0x04, 0xAD, 0xA9, 0x00, 0x04, 0xBE, 0xF7,
            0x00, 0x04, 0xD7, 0x94, 0x00, 0x04, 0xED, 0x6C, 0x00, 0x05, 0x0E, 0x3B,
            0x00, 0x05, 0x2D, 0xC9, 0x00, 0x05, 0x59, 0xF0, 0x00, 0x05, 0x7B, 0x82,
            0x00, 0x05, 0x95, 0x2B, 0x00, 0x05, 0xB3, 0xDA, 0x00, 0x05, 0xC6, 0x67,
            0x00, 0x00, 0x00, 0x14, 0x73, 0x74, 0x73, 0x73, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xB2,
            0x73, 0x64, 0x74, 0x70, 0x00, 0x00, 0x00, 0x00, 0x04, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44,
            0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x00, 0x00, 0x06, 0x7E, 0x74, 0x72,
            0x61, 0x6B, 0x00, 0x00, 0x00, 0x5C, 0x74, 0x6B, 0x68, 0x64, 0x00, 0x00,
            0x00, 0x03, 0xC7, 0xCA, 0xEE, 0xA7, 0xC7, 0xCA, 0xEE, 0xA8, 0x00, 0x00,
            0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0xA5, 0x80, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x06, 0x04, 0x6D, 0x64, 0x69, 0x61, 0x00, 0x00, 0x00, 0x20, 0x6D, 0x64,
            0x68, 0x64, 0x00, 0x00, 0x00, 0x00, 0xC7, 0xCA, 0xEE, 0xA7, 0xC7, 0xCA,
            0xEE, 0xA8, 0x00, 0x00, 0xBB, 0x80, 0x00, 0x04, 0x14, 0x00, 0x15, 0xC7,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x68, 0x64, 0x6C, 0x72, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0x6F, 0x75, 0x6E, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x05, 0xBB, 0x6D, 0x69, 0x6E, 0x66, 0x00, 0x00, 0x00, 0x10, 0x73,
            0x6D, 0x68, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x24, 0x64, 0x69, 0x6E, 0x66, 0x00, 0x00, 0x00, 0x1C, 0x64,
            0x72, 0x65, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x0C, 0x75, 0x72, 0x6C, 0x20, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x05, 0x7F, 0x73, 0x74, 0x62, 0x6C, 0x00, 0x00, 0x00, 0x67, 0x73,
            0x74, 0x73, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x57, 0x6D, 0x70, 0x34, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x80, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x33, 0x65, 0x73, 0x64, 0x73, 0x00, 0x00, 0x00, 0x00, 0x03,
            0x80, 0x80, 0x80, 0x22, 0x00, 0x00, 0x00, 0x04, 0x80, 0x80, 0x80, 0x14,
            0x40, 0x15, 0x00, 0x01, 0x18, 0x00, 0x01, 0x65, 0xF0, 0x00, 0x01, 0x44,
            0x6B, 0x05, 0x80, 0x80, 0x80, 0x02, 0x11, 0x88, 0x06, 0x80, 0x80, 0x80,
            0x01, 0x02, 0x00, 0x00, 0x00, 0x18, 0x73, 0x74, 0x74, 0x73, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x05, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00, 0x04, 0x28, 0x73, 0x74, 0x73, 0x7A, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x05, 0x00, 0x00,
            0x00, 0xF7, 0x00, 0x00, 0x00, 0xDB, 0x00, 0x00, 0x00, 0xE1, 0x00, 0x00,
            0x00, 0xE5, 0x00, 0x00, 0x00, 0xE9, 0x00, 0x00, 0x00, 0xE8, 0x00, 0x00,
            0x00, 0xF0, 0x00, 0x00, 0x00, 0xF1, 0x00, 0x00, 0x00, 0xEF, 0x00, 0x00,
            0x00, 0xD8, 0x00, 0x00, 0x00, 0xE6, 0x00, 0x00, 0x00, 0xE7, 0x00, 0x00,
            0x00, 0xE9, 0x00, 0x00, 0x00, 0xEB, 0x00, 0x00, 0x00, 0xEA, 0x00, 0x00,
            0x00, 0xE8, 0x00, 0x00, 0x00, 0xE1, 0x00, 0x00, 0x00, 0xE7, 0x00, 0x00,
            0x00, 0xD7, 0x00, 0x00, 0x00, 0xDA, 0x00, 0x00, 0x00, 0xD9, 0x00, 0x00,
            0x00, 0xDB, 0x00, 0x00, 0x00, 0xE9, 0x00, 0x00, 0x00, 0xEE, 0x00, 0x00,
            0x00, 0xE5, 0x00, 0x00, 0x00, 0xE1, 0x00, 0x00, 0x00, 0xE6, 0x00, 0x00,
            0x00, 0xE5, 0x00, 0x00, 0x00, 0xD8, 0x00, 0x00, 0x00, 0xDD, 0x00, 0x00,
            0x00, 0xDD, 0x00, 0x00, 0x00, 0xD5, 0x00, 0x00, 0x00, 0xEA, 0x00, 0x00,
            0x00, 0xDD, 0x00, 0x00, 0x00, 0xD0, 0x00, 0x00, 0x00, 0xD6, 0x00, 0x00,
            0x00, 0xE9, 0x00, 0x00, 0x00, 0xBC, 0x00, 0x00, 0x00, 0xAB, 0x00, 0x00,
            0x00, 0xB3, 0x00, 0x00, 0x00, 0xB5, 0x00, 0x00, 0x00, 0xBC, 0x00, 0x00,
            0x00, 0xCE, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0xB6, 0x00, 0x00,
            0x00, 0xB3, 0x00, 0x00, 0x00, 0xB6, 0x00, 0x00, 0x00, 0xB7, 0x00, 0x00,
            0x00, 0xBF, 0x00, 0x00, 0x00, 0xB7, 0x00, 0x00, 0x00, 0xCD, 0x00, 0x00,
            0x00, 0xC1, 0x00, 0x00, 0x00, 0xBA, 0x00, 0x00, 0x00, 0xA7, 0x00, 0x00,
            0x00, 0xB4, 0x00, 0x00, 0x00, 0xB1, 0x00, 0x00, 0x00, 0xBE, 0x00, 0x00,
            0x00, 0xD0, 0x00, 0x00, 0x00, 0xBA, 0x00, 0x00, 0x00, 0xBC, 0x00, 0x00,
            0x00, 0xC4, 0x00, 0x00, 0x00, 0xC6, 0x00, 0x00, 0x00, 0xCB, 0x00, 0x00,
            0x00, 0xC4, 0x00, 0x00, 0x00, 0xC3, 0x00, 0x00, 0x00, 0xC8, 0x00, 0x00,
            0x00, 0xD2, 0x00, 0x00, 0x00, 0xD2, 0x00, 0x00, 0x00, 0xD6, 0x00, 0x00,
            0x00, 0xF5, 0x00, 0x00, 0x00, 0xFA, 0x00, 0x00, 0x00, 0xF6, 0x00, 0x00,
            0x01, 0x02, 0x00, 0x00, 0x00, 0xFC, 0x00, 0x00, 0x00, 0xFC, 0x00, 0x00,
            0x00, 0xEE, 0x00, 0x00, 0x00, 0xE6, 0x00, 0x00, 0x00, 0xEA, 0x00, 0x00,
            0x00, 0xEA, 0x00, 0x00, 0x00, 0xE8, 0x00, 0x00, 0x00, 0xDE, 0x00, 0x00,
            0x00, 0xDF, 0x00, 0x00, 0x00, 0xE7, 0x00, 0x00, 0x00, 0xF6, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x01, 0x03, 0x00, 0x00, 0x00, 0xF6, 0x00, 0x00,
            0x01, 0x08, 0x00, 0x00, 0x01, 0x03, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00,
            0x01, 0x05, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x14, 0x00, 0x00, 0x01, 0x18, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00,
            0x00, 0xFB, 0x00, 0x00, 0x01, 0x11, 0x00, 0x00, 0x01, 0x05, 0x00, 0x00,
            0x01, 0x05, 0x00, 0x00, 0x01, 0x0A, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00,
            0x00, 0xF3, 0x00, 0x00, 0x00, 0xF7, 0x00, 0x00, 0x00, 0xF7, 0x00, 0x00,
            0x01, 0x01, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0xF8, 0x00, 0x00,
            0x00, 0xF8, 0x00, 0x00, 0x00, 0xEF, 0x00, 0x00, 0x00, 0xED, 0x00, 0x00,
            0x00, 0xE3, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00, 0x00, 0xE2, 0x00, 0x00,
            0x00, 0xE8, 0x00, 0x00, 0x00, 0xDC, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00,
            0x00, 0xF3, 0x00, 0x00, 0x00, 0xDF, 0x00, 0x00, 0x00, 0xE1, 0x00, 0x00,
            0x00, 0xCF, 0x00, 0x00, 0x00, 0xCE, 0x00, 0x00, 0x00, 0xD8, 0x00, 0x00,
            0x00, 0xCE, 0x00, 0x00, 0x00, 0xC7, 0x00, 0x00, 0x00, 0xCD, 0x00, 0x00,
            0x00, 0xB7, 0x00, 0x00, 0x00, 0xAF, 0x00, 0x00, 0x00, 0xC8, 0x00, 0x00,
            0x00, 0xD7, 0x00, 0x00, 0x00, 0xE5, 0x00, 0x00, 0x00, 0xE4, 0x00, 0x00,
            0x00, 0xC6, 0x00, 0x00, 0x00, 0xD1, 0x00, 0x00, 0x00, 0xD5, 0x00, 0x00,
            0x00, 0xE5, 0x00, 0x00, 0x00, 0xD8, 0x00, 0x00, 0x00, 0xC8, 0x00, 0x00,
            0x00, 0xBE, 0x00, 0x00, 0x00, 0xBF, 0x00, 0x00, 0x00, 0xCB, 0x00, 0x00,
            0x00, 0xD2, 0x00, 0x00, 0x00, 0xC8, 0x00, 0x00, 0x00, 0xCA, 0x00, 0x00,
            0x00, 0xB1, 0x00, 0x00, 0x00, 0xA3, 0x00, 0x00, 0x00, 0xC7, 0x00, 0x00,
            0x00, 0xDC, 0x00, 0x00, 0x00, 0xD9, 0x00, 0x00, 0x00, 0xDD, 0x00, 0x00,
            0x00, 0xD1, 0x00, 0x00, 0x00, 0xD2, 0x00, 0x00, 0x00, 0xC2, 0x00, 0x00,
            0x00, 0xBC, 0x00, 0x00, 0x00, 0xB1, 0x00, 0x00, 0x00, 0x9B, 0x00, 0x00,
            0x00, 0x89, 0x00, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x00, 0x9F, 0x00, 0x00,
            0x00, 0xB5, 0x00, 0x00, 0x00, 0xA6, 0x00, 0x00, 0x00, 0xB2, 0x00, 0x00,
            0x00, 0xB5, 0x00, 0x00, 0x00, 0xAE, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00,
            0x00, 0xB0, 0x00, 0x00, 0x00, 0xC6, 0x00, 0x00, 0x00, 0xC3, 0x00, 0x00,
            0x00, 0xD5, 0x00, 0x00, 0x00, 0xE4, 0x00, 0x00, 0x00, 0xF6, 0x00, 0x00,
            0x00, 0xD6, 0x00, 0x00, 0x00, 0xDB, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00,
            0x00, 0xE7, 0x00, 0x00, 0x00, 0xF9, 0x00, 0x00, 0x00, 0xCB, 0x00, 0x00,
            0x00, 0xD8, 0x00, 0x00, 0x00, 0xD6, 0x00, 0x00, 0x00, 0xE4, 0x00, 0x00,
            0x00, 0xF1, 0x00, 0x00, 0x00, 0xE4, 0x00, 0x00, 0x00, 0xE6, 0x00, 0x00,
            0x00, 0xDF, 0x00, 0x00, 0x00, 0xEE, 0x00, 0x00, 0x00, 0xD7, 0x00, 0x00,
            0x00, 0xC7, 0x00, 0x00, 0x00, 0xE7, 0x00, 0x00, 0x00, 0xF9, 0x00, 0x00,
            0x00, 0xED, 0x00, 0x00, 0x00, 0xCF, 0x00, 0x00, 0x00, 0xF1, 0x00, 0x00,
            0x00, 0xE6, 0x00, 0x00, 0x00, 0xDC, 0x00, 0x00, 0x00, 0xE4, 0x00, 0x00,
            0x00, 0xEF, 0x00, 0x00, 0x00, 0xE5, 0x00, 0x00, 0x00, 0xF1, 0x00, 0x00,
            0x00, 0xE3, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00,
            0x00, 0xF3, 0x00, 0x00, 0x00, 0xF5, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00,
            0x01, 0x0B, 0x00, 0x00, 0x01, 0x10, 0x00, 0x00, 0x01, 0x11, 0x00, 0x00,
            0x01, 0x03, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0xFB, 0x00, 0x00,
            0x00, 0xFA, 0x00, 0x00, 0x00, 0xE7, 0x00, 0x00, 0x00, 0xE5, 0x00, 0x00,
            0x00, 0xF0, 0x00, 0x00, 0x00, 0xD2, 0x00, 0x00, 0x00, 0xE5, 0x00, 0x00,
            0x00, 0xF3, 0x00, 0x00, 0x00, 0xF1, 0x00, 0x00, 0x00, 0xF2, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xF7, 0x00, 0x00, 0x00, 0xEE, 0x00, 0x00,
            0x00, 0xD5, 0x00, 0x00, 0x00, 0xD9, 0x00, 0x00, 0x00, 0xEA, 0x00, 0x00,
            0x00, 0xE3, 0x00, 0x00, 0x00, 0xDF, 0x00, 0x00, 0x00, 0xF7, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xF8, 0x00, 0x00, 0x00, 0xFA, 0x00, 0x00,
            0x00, 0xFD, 0x00, 0x00, 0x00, 0xF7, 0x00, 0x00, 0x00, 0xF9, 0x00, 0x00,
            0x00, 0xFB, 0x00, 0x00, 0x00, 0xF8, 0x00, 0x00, 0x00, 0xF6, 0x00, 0x00,
            0x00, 0xF0, 0x00, 0x00, 0x00, 0xFE, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
            0x00, 0xE9, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00,
            0x00, 0xE7, 0x00, 0x00, 0x00, 0xEA, 0x00, 0x00, 0x00, 0xDE, 0x00, 0x00,
            0x00, 0xE2, 0x00, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x00, 0xD4, 0x00, 0x00,
            0x00, 0xD4, 0x00, 0x00, 0x00, 0xC7, 0x00, 0x00, 0x00, 0xC9, 0x00, 0x00,
            0x00, 0xC8, 0x00, 0x00, 0x00, 0xC1, 0x00, 0x00, 0x00, 0xC0, 0x00, 0x00,
            0x00, 0xBD, 0x00, 0x00, 0x00, 0xDE, 0x00, 0x00, 0x00, 0xCB, 0x00, 0x00,
            0x00, 0xCD, 0x00, 0x00, 0x00, 0xD4, 0x00, 0x00, 0x00, 0x6D, 0x00, 0x00,
            0x00, 0x28, 0x73, 0x74, 0x73, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x26, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0xA8, 0x73, 0x74, 0x63, 0x6F, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x26, 0x00, 0x00, 0x6D, 0x8D, 0x00, 0x00,
            0x85, 0x9B, 0x00, 0x00, 0x9D, 0xE4, 0x00, 0x00, 0xB1, 0x21, 0x00, 0x00,
            0xC2, 0xA7, 0x00, 0x00, 0xD2, 0x8E, 0x00, 0x00, 0xDF, 0x8E, 0x00, 0x00,
            0xF3, 0x54, 0x00, 0x00, 0xFE, 0xE8, 0x00, 0x01, 0x1A, 0x05, 0x00, 0x01,
            0x4A, 0xC7, 0x00, 0x01, 0x7E, 0x28, 0x00, 0x01, 0xC5, 0xA1, 0x00, 0x01,
            0xFC, 0x89, 0x00, 0x02, 0x35, 0x5C, 0x00, 0x02, 0x83, 0x6A, 0x00, 0x02,
            0xAE, 0x62, 0x00, 0x02, 0xE1, 0xAE, 0x00, 0x03, 0x1D, 0x6D, 0x00, 0x03,
            0x58, 0x04, 0x00, 0x03, 0x99, 0x4D, 0x00, 0x03, 0xE4, 0xB1, 0x00, 0x04,
            0x21, 0x99, 0x00, 0x04, 0x39, 0x67, 0x00, 0x04, 0x5D, 0xC6, 0x00, 0x04,
            0x78, 0x18, 0x00, 0x04, 0x92, 0x6A, 0x00, 0x04, 0xA7, 0x67, 0x00, 0x04,
            0xB8, 0x7E, 0x00, 0x04, 0xD0, 0x6C, 0x00, 0x04, 0xE7, 0x0C, 0x00, 0x05,
            0x07, 0xC6, 0x00, 0x05, 0x53, 0x5C, 0x00, 0x05, 0x74, 0xBC, 0x00, 0x05,
            0x8E, 0x99, 0x00, 0x05, 0xAE, 0x19, 0x00, 0x05, 0xC0, 0xEB, 0x00, 0x05,
            0xCB, 0x47, 0x00, 0x00, 0x00, 0x16, 0x75, 0x64, 0x74, 0x61, 0x00, 0x00,
            0x00, 0x0E, 0x6E, 0x61, 0x6D, 0x65, 0x53, 0x74, 0x65, 0x72, 0x65, 0x6F,
            0x00, 0x00, 0x00, 0x6F, 0x75, 0x64, 0x74, 0x61, 0x00, 0x00, 0x00, 0x67,
            0x6D, 0x65, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21,
            0x68, 0x64, 0x6C, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x6D, 0x64, 0x69, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3A, 0x69, 0x6C, 0x73,
            0x74, 0x00, 0x00, 0x00, 0x32, 0xA9, 0x74, 0x6F, 0x6F, 0x00, 0x00, 0x00,
            0x2A, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x48, 0x61, 0x6E, 0x64, 0x42, 0x72, 0x61, 0x6B, 0x65, 0x20, 0x30,
            0x2E, 0x39, 0x2E, 0x34, 0x20, 0x32, 0x30, 0x30, 0x39, 0x31, 0x31, 0x32,
            0x33, 0x30, 0x30
        };
    }
}
