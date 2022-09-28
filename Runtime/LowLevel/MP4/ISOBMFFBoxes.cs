using MediaFramework.LowLevel.Unsafe;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MediaFramework.LowLevel.MP4
{
    // 4.2 ISO/IEC 14496-12:2015(E)
    public struct ISOBox
    {
        public const int ByteNeeded = 8;

        public uint size;
        public ISOBoxType type;
    }

    public struct ISODate
    {
        public ulong value;

        public static implicit operator ISODate(ulong d) => new ISODate { value = d };
    }

    public struct SampleGroup
    {
        public uint Count;
        public uint Delta;
    }

    public static class ISOBMFF
    {
        const int MaxBoxDepth = 10;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            if (context.BoxDepth > MaxBoxDepth) 
            {
                logger.LogError(context.Tag, $"{MP4Error.IllegalBoxDepth}: Box chain is nested too depth >{MaxBoxDepth}");
                return MP4Error.IllegalBoxDepth;
            }

            var error = MP4Error.None;

            context.BoxDepth++;

            int lastBytePosition = (int)box.size + reader.Index - ISOBox.ByteNeeded;
            if (lastBytePosition > reader.Length) 
            {
                logger.LogError(context.Tag, $"{MP4Error.IndexOutOfReaderRange}: {box.type} ({box.size}) is greater than reader length");
                return MP4Error.IndexOutOfReaderRange;
            }

            while (reader.Index + ISOBox.ByteNeeded < lastBytePosition)
            {
                var start = reader.Index;
                var isoBox = reader.ReadISOBox();

                if (isoBox.size + start > lastBytePosition)
                {
                    logger.LogError(context.Tag, $"{MP4Error.IndexOutOfReaderRange}: {isoBox.type} ({isoBox.size}) is greater than {box.type}. Missing {isoBox.size + start - lastBytePosition} bytes");
                    return MP4Error.IndexOutOfReaderRange;
                }

                switch (isoBox.type)
                {
                    case ISOBoxType.STBL:
                    case ISOBoxType.MINF:
                    case ISOBoxType.MDIA: error = Read(ref context, ref reader, ref logger, isoBox); break;

                    case ISOBoxType.MVHD: error = MVHDBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.MDHD: error = MDHDBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.TRAK: error = TRAKBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.TKHD: error = TKHDBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.HDLR: error = HDLRBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.STSD: error = STSDBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.AVCC: error = AVCCBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.STTS: error = STTSBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.STSC: error = STSCBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    case ISOBoxType.STCO: error = STCOBox.Read(ref context, ref reader, ref logger, isoBox); break;
                    default: 
                        reader.Seek((int)isoBox.size - ISOBox.ByteNeeded); 
                        break;
                }

                if (error != MP4Error.None)
                    return error;

                // //Assert.AreEqual((int)isoBox.size, reader.Index - start, $"{isoBox.type} didn't read all the bytes");
            }

            context.BoxDepth--;

            return error;
        }
    }

    public static class VisualSampleEntry
    {
        public const int Size = 86;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            if (box.size == Size)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size is {Size} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            ref var video = ref context.VideoList.ElementAt(context.VideoList.Length - 1);

            video.CodecTag = (uint)box.type;
            reader.Seek(6);
            video.ReferenceIndex = reader.ReadUInt16();
            reader.Seek(4 * 4); // pre_defined[4]
            video.Width = reader.ReadUInt16();
            video.Height = reader.ReadUInt16();
            reader.Seek(4); // horizresolution
            reader.Seek(4); // vertresolution
            reader.Seek(4); // reserved
            reader.Seek(2); // frame_count
            reader.Seek(32); // compressorname
            video.Depth = reader.ReadUInt16();
            reader.Seek(2); // pre_defined

            return MP4Error.None;
        }
    }

    /// <summary>
    /// AudioSpecificConfig ISO 14496-3 Section 1.6.2.1
    /// </summary>
    public static class ESDSBox
    {
        public const int HeaderSize = 12;
        public const int MinSize = 25;

        public unsafe static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.MVHD, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            ref var audio = ref context.LastAudio;

            if (audio.Samplerate != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            reader.Seek(4); // version + flags

            var bitReader = new BBitReader(reader.GetUnsafePtr(), (int)box.size - 12);

            ReaderError err;
            if ((err = bitReader.TryReadBits(5, out var audioObjectType)) != ReaderError.None)
                return (MP4Error)err;

            if (audioObjectType == 31)
            {
                if ((err = bitReader.TryReadBits(6, out var audioObjectTypeExt)) != ReaderError.None)
                    return (MP4Error)err;

                //audioObjectType = 32 + audioObjectTypeExt;
            }

            if ((err = bitReader.TryReadBits(4, out var samplingFrequencyIndex)) != ReaderError.None)
                return (MP4Error)err;

            if (samplingFrequencyIndex != 15)
            {
                audio.Samplerate = samplingFrequencyIndex switch
                {
                    0 => 96000,
                    1 => 88200,
                    2 => 64000,
                    3 => 48000,
                    4 => 44100,
                    5 => 32000,
                    6 => 24000,
                    7 => 22050,
                    8 => 16000,
                    9 => 12000,
                    10 => 11025,
                    11 => 8000,
                    12 => 7350,
                    _ => 0
                };
            }
            else
            {
                if ((err = bitReader.TryReadBits(4, out var samplerate)) != ReaderError.None)
                    return (MP4Error)err;

                audio.Samplerate = (int)samplerate;
            }

            if ((err = bitReader.TryReadBits(4, out var channelConfiguration)) != ReaderError.None)
                return (MP4Error)err;

            audio.ChannelCount = (int)channelConfiguration;

            reader.Seek((int)box.size - HeaderSize);
            return MP4Error.None;
        }
    }

    /// <summary>
    /// AVCDecoderConfigurationRecord ISO/IEC 14496-15:2010(E)
    /// </summary>
    public static class AVCCBox
    {
        public const int MinSize = 14;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            // //Assert.AreEqual(ISOBoxType.AVCC, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.GetTag(1), $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            //ref var video = ref context.LastVideo;

            //if (video.Profile.Type != 0)
            //{
            //    logger.LogError(context.GetTag(2), $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
            //    return MP4Error.DuplicateBox;
            //}

            //video.CodecID = VideoCodec.H264;

            //reader.Seek(1); // configurationVersion
            //video.Profile.Type = reader.ReadUInt8();
            //video.Profile.Constraints = reader.ReadUInt8();
            //video.Profile.Level = reader.ReadUInt8();
            //reader.Seek(1); // lengthSizeMinusOne

            //video.SPS.Offset = reader.Index;
            //int numSPS = reader.ReadUInt8() & 0b00011111;
            //int remains = (int)box.size - MinSize;
            //for (int i = 0; i < numSPS; i++)
            //{
            //    if (remains < 2)
            //    {
            //        logger.LogError(context.GetTag(3), $"{MP4Error.InvalidBoxSize}: {box.type} size was too small to read SPS length");
            //        return MP4Error.InvalidBoxSize;
            //    }
            //    remains -= 2;

            //    int spsLength = reader.ReadUInt16();

            //    if (remains < spsLength)
            //    {
            //        logger.LogError(context.GetTag(4), $"{MP4Error.InvalidBoxSize}: {box.type} size was too small to read SPS");
            //        return MP4Error.InvalidBoxSize;
            //    }
            //    remains -= spsLength;

            //    reader.Seek(spsLength);
            //}
            //video.SPS.Length = reader.Index - video.SPS.Offset;

            //if (remains < 1)
            //{
            //    logger.LogError(context.GetTag(5), $"{MP4Error.InvalidBoxSize}: {box.type} size was too small to read the number of PPS");
            //    return MP4Error.InvalidBoxSize;
            //}
            //remains -= 1;

            //video.PPS.Offset = reader.Index;
            //int numPPS = reader.ReadUInt8();
            //for (int i = 0; i < numPPS; i++)
            //{
            //    if (remains < 2)
            //    {
            //        logger.LogError(context.GetTag(6), $"{MP4Error.InvalidBoxSize}: {box.type} size was too small to read PPS length");
            //        return MP4Error.InvalidBoxSize;
            //    }
            //    remains -= 2;

            //    int ppsLength = reader.ReadUInt16();

            //    if (remains < ppsLength)
            //    {
            //        logger.LogError(context.GetTag(7), $"{MP4Error.InvalidBoxSize}: {box.type} size was too small to read PPS");
            //        return MP4Error.InvalidBoxSize;
            //    }
            //    remains -= ppsLength;

            //    reader.Seek(ppsLength);
            //}
            //video.PPS.Length = reader.Index - video.PPS.Offset;

            // The standard mention we can have the chroma subsampling and the bit depth here.
            // But unfortunetly, it seems to inconsistent so I prefer to read the SPS if needed.
            reader.Seek((int)box.size - ISOBox.ByteNeeded);

            return MP4Error.None;
        }
    }

    public static class MVHDBox
    {
        public const int Version0 = 108;
        public const int Version1 = 120;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.MVHD, box.type, "ISOBoxType");

            if (box.size < Version0)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {Version0} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            if (context.Duration != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            var version = reader.ReadUInt8();
            reader.Seek(3); // flags

            switch (version)
            {
                case 0:
                    if (box.size != Version0)
                    {
                        logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size for version 0 is {Version0} but was {box.size}");
                        return MP4Error.InvalidBoxSize;
                    }

                    context.CreationTime = reader.ReadUInt32();
                    context.ModificationTime = reader.ReadUInt32();
                    context.Timescale = reader.ReadUInt32();
                    context.Duration = reader.ReadUInt32();
                    break;
                case 1:
                    if (box.size != Version1)
                    {
                        logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size for version 1 is {Version1} but was {box.size}");
                        return MP4Error.InvalidBoxSize;
                    }

                    context.CreationTime = reader.ReadUInt64();
                    context.ModificationTime = reader.ReadUInt64();
                    context.Timescale = reader.ReadUInt32();
                    context.Duration = reader.ReadUInt64();
                    break;
                default:
                    logger.LogError(context.Tag, $"{MP4Error.InvalidBoxVersion}: {box.type} has an invalid version of {version}");
                    return MP4Error.InvalidBoxVersion;
            }

            reader.Seek(4); // rate
            reader.Seek(2); // volume  
            reader.Seek(2); // reserved   
            reader.Seek(4 * 2); // reserved  
            reader.Seek(4 * 9); // matrix
            reader.Seek(4 * 6); // pre_defined 
            context.NextTrackID = reader.ReadUInt32();

            return MP4Error.None;
        }
    }

    public static class TRAKBox
    {
        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            ////Assert.AreEqual(ISOBoxType.TRAK, box.type, "ISOBoxType");

            // We wait until we start reading a track so we can minimize
            // the chance of resizing the list during the parsing
            if (!context.TrackList.IsCreated)
            {
                var capacity = context.NextTrackID > 0 ? (int)context.NextTrackID - 1 : 8;
                context.TrackList = new UnsafeList<MP4TrackContext>(capacity, Allocator.Temp);
            }

            context.TrackList.Add(new MP4TrackContext());

            var error = ISOBMFF.Read(ref context, ref reader, ref logger, box);

            ref var track = ref context.LastTrack;

            return error;
        }   
    }

    public static class TKHDBox
    {
        public const int Version0 = 92;
        public const int Version1 = 104;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.TKHD, box.type, "ISOBoxType");

            if (box.size < Version0)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {Version0} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            if (track.TrackID != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            var version = reader.ReadUInt8();
            reader.Seek(3); // flags

            switch (version)
            {
                case 0:
                    if (box.size != Version0)
                    {
                        logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size for version 0 is {Version0} but was {box.size}");
                        return MP4Error.InvalidBoxSize;
                    }

                    reader.Seek(4); // creation_time
                    reader.Seek(4); // modification_time
                    track.TrackID = reader.ReadUInt32();
                    reader.Seek(4); // reserved
                    reader.Seek(4); // duration
                    break;
                case 1:
                    if (box.size != Version1)
                    {
                        logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size for version 1 is {Version1} but was {box.size}");
                        return MP4Error.InvalidBoxSize;
                    }

                    reader.Seek(8); // creation_time
                    reader.Seek(8); // modification_time
                    track.TrackID = reader.ReadUInt32();
                    reader.Seek(4); // reserved
                    reader.Seek(8); // duration
                    break;
                default:
                    logger.LogError(context.Tag, $"{MP4Error.InvalidBoxVersion}: {box.type} has an invalid version of {version}");
                    return MP4Error.InvalidBoxVersion;
            }

            reader.Seek(8); // reserved
            reader.Seek(2); // layer 
            reader.Seek(2); // alternate_group  
            reader.Seek(2); // volume 
            reader.Seek(2); // reserved
            reader.Seek(4 * 9); // matrix
            reader.Seek(4); // width
            reader.Seek(4); // height

            return MP4Error.None;
        }
    }

    public static class MDHDBox
    {
        public const int Version0 = 32;
        public const int Version1 = 44;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.MDHD, box.type, "ISOBoxType");

            if (box.size < Version0)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {Version0} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            if (track.Duration != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            var version = reader.ReadUInt8();
            reader.Seek(3); // flags

            switch (version)
            {
                case 0:
                    if (box.size != Version0)
                    {
                        logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size for version 0 is {Version0} but was {box.size}");
                        return MP4Error.InvalidBoxSize;
                    }

                    reader.Seek(4); // creation_time
                    reader.Seek(4); // modification_time
                    track.Timescale = reader.ReadUInt32();
                    track.Duration = reader.ReadUInt32();
                    break;
                case 1:
                    if (box.size != Version1)
                    {
                        logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} size for version 1 is {Version1} but was {box.size}");
                        return MP4Error.InvalidBoxSize;
                    }

                    reader.Seek(8); // creation_time
                    reader.Seek(8); // modification_time
                    track.Timescale = reader.ReadUInt32();
                    track.Duration = reader.ReadUInt64();
                    break;
                default:
                    logger.LogError(context.Tag, $"{MP4Error.InvalidBoxVersion}: {box.type} has an invalid version of {version}");
                    return MP4Error.InvalidBoxVersion;
            }

            track.Language = (ISOLanguage)reader.ReadUInt16();
            reader.Seek(2); // pre_defined 

            return MP4Error.None;
        }
    }

    public static class HDLRBox
    {
        public const int MinSize = 33;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.HDLR, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            if (track.Handler != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            reader.Seek(4); // version + flags
            reader.Seek(4); // pre_defined
            track.Handler = (ISOHandler)reader.ReadUInt32();
            reader.Seek(4 * 3); // reserved
            reader.Seek(1); // 1 null terminator

            reader.Seek((int)box.size - MinSize);

            return MP4Error.None;
        }
    }

    public static class STSDBox
    {
        public const int MinSize = 32;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.STSD, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            reader.Seek(4); // version + flags
            var entries = reader.ReadUInt32();

            if (entries == 0 || entries > 256)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidEntryCount}: {box.type} invalid entries {entries}");
                return MP4Error.InvalidEntryCount;
            }

            track.Descriptions.Offset = context.VideoList.Length;
            track.Descriptions.Length = (int)entries;

            var error = MP4Error.None;
            switch (track.Handler)
            {
                case ISOHandler.VIDE:
                    for (int i = 0; i < entries; i++)
                    {
                        context.VideoList.Add(new MP4VideoDescription());

                        var start = reader.Index;
                        var isoBox = reader.ReadISOBox();
                        error = VisualSampleEntry.Read(ref context, ref reader, ref logger, isoBox);
                        if (error != MP4Error.None)
                            return error;

                        context.LastVideo.Extra 
                            = reader.SeekArray((int)isoBox.size - (reader.Index - start));
                    }
                    break;
                case ISOHandler.SOUN:
                    reader.Seek((int)box.size - 16);
                    break;
                default:
                    break;
            }

            return error;
        }
    }

    public static class STTSBox
    {
        public const int HeaderSize = 16;
        public const int SampleSize = 8;
        public const int MinSize = HeaderSize + SampleSize;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.STTS, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            if (((box.size - HeaderSize) % SampleSize) != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: Each {box.type} sample needs to be {SampleSize}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            if (track.STTS.EntryCount != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            reader.Seek(4); // version + flags

            track.STTS.EntryCount = (int)reader.ReadUInt32();
            if ((box.size - HeaderSize) / SampleSize != track.STTS.EntryCount)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidEntryCount}: {box.type} read {track.STTS.EntryCount} entries but was {(box.size - HeaderSize) / SampleSize}");
                return MP4Error.InvalidEntryCount;
            }

            track.STTS.SampleIndex = reader.Index;

            reader.Seek((int)box.size - HeaderSize);
            return MP4Error.None;
        }
    }

    public static class STSCBox
    {
        public const int HeaderSize = 16;
        public const int SampleSize = 12;
        public const int MinSize = HeaderSize + SampleSize;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.STSC, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            if (((box.size - HeaderSize) % SampleSize) != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: Each {box.type} sample needs to be {SampleSize}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            if (track.STSC.EntryCount != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            reader.Seek(4); // version + flags

            track.STSC.EntryCount = (int)reader.ReadUInt32();
            if ((box.size - HeaderSize) / SampleSize != track.STSC.EntryCount)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidEntryCount}: {box.type} read {track.STSC.EntryCount} entries but was {(box.size - HeaderSize) / SampleSize}");
                return MP4Error.InvalidEntryCount;
            }

            track.STSC.SampleIndex = reader.Index;

            reader.Seek((int)box.size - HeaderSize);
            return MP4Error.None;
        }
    }

    public static class STCOBox
    {
        public const int HeaderSize = 16;
        public const int SampleSize = 4;
        public const int MinSize = HeaderSize + SampleSize;

        public static MP4Error Read(ref MP4Context context, ref BByteReader reader, ref JobLogger logger, in ISOBox box)
        {
            //Assert.AreEqual(ISOBoxType.STCO, box.type, "ISOBoxType");

            if (box.size < MinSize)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: {box.type} minimum size is {MinSize} but was {box.size}");
                return MP4Error.InvalidBoxSize;
            }

            if (((box.size - HeaderSize) % SampleSize) != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidBoxSize}: Each {box.type} sample needs to be {SampleSize}");
                return MP4Error.InvalidBoxSize;
            }

            ref var track = ref context.LastTrack;

            if (track.STCO.EntryCount != 0)
            {
                logger.LogError(context.Tag, $"{MP4Error.DuplicateBox}: Duplicate {box.type} box detected");
                return MP4Error.DuplicateBox;
            }

            reader.Seek(4); // version + flags

            track.STCO.EntryCount = (int)reader.ReadUInt32();
            if ((box.size - HeaderSize) / SampleSize != track.STCO.EntryCount)
            {
                logger.LogError(context.Tag, $"{MP4Error.InvalidEntryCount}: {box.type} read {track.STCO.EntryCount} entries but was {(box.size - HeaderSize) / SampleSize}");
                return MP4Error.InvalidEntryCount;
            }

            track.STCO.SampleIndex = reader.Index;

            reader.Seek((int)box.size - HeaderSize);
            return MP4Error.None;
        }
    }

    public static class MP4Math
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvertFixedPoint16(int value) => value >> 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertFixedPoint16(uint value) => value >> 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvertFixedPoint8(int value) => value >> 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertFixedPoint8(uint value) => value >> 8;
    }

    public static class MP4BoxUtility
    {
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckForBoxType(ISOBoxType value, ISOBoxType compare)
        {
            if (value != compare)
                throw new ArgumentException($"The box type was {value} but expected {compare} box");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckIfReaderRead(int index, in BByteReader reader, in ISOBox box)
        {
            if(index >= reader.Index)
                throw new InvalidOperationException($"Infinite loop detected. The reader didn't progress when reading {box.type} box");
        }
    }

    // Useful to convert FourCC to HEX
    // https://www.branah.com/ascii-converter

    /// <summary>
    /// FourCC boxes. The value is the FourCC in decimal
    /// </summary>
    public enum ISOBoxType : uint
    {
        BXML = 0x62786D6C,
        CO64 = 0x636f3634,
        CPRT = 0x63707274,
        CSLG = 0x63736c67,
        CTTS = 0x63747473,
        DINF = 0x64696e66,
        DREF = 0x64726566,
        EDTS = 0x65647473,
        ELNG = 0x656c6e67,
        ELST = 0x656c7374,
        FECR = 0x66656372,
        FIIN = 0x6669696E,
        FIRE = 0x66697265,
        FPAR = 0x66706172,
        FREE = 0x66726565,
        FRMA = 0x66726D61,
        FTYP = 0x66747970,
        GITN = 0x6769746E,
        HDLR = 0x68646c72,
        HMHD = 0x686d6864,
        IDAT = 0x69646174,
        IINF = 0x69696E66,
        ILOC = 0x696C6F63,
        IPRO = 0x6970726F,
        IREF = 0x69726566,
        LEVA = 0x6c657661,
        MDAT = 0x6d646174,
        MDHD = 0x6d646864,
        MDIA = 0x6d646961,
        MECO = 0x6D65636F,
        MEHD = 0x6d656864,
        MERE = 0x6D657265,
        META = 0x6d657461,
        MFHD = 0x6D666864,
        MFRA = 0x6D667261,
        MFRO = 0x6D66726F,
        MINF = 0x6d696e66,
        MOOF = 0x6D6F6F66,
        MOOV = 0x6d6f6f76,
        MVEX = 0x6d766578,
        MVHD = 0x6d766864,
        NMHD = 0x6e6d6864,
        PADB = 0x70616462,
        PAEN = 0x7061656E,
        PDIN = 0x7064696e,
        PITM = 0x7069746D,
        PRFT = 0x70726674,
        SAIO = 0x7361696f,
        SAIZ = 0x7361697a,
        SBGP = 0x73626770,
        SCHI = 0x73636869,
        SCHM = 0x7363686D,
        SDTP = 0x73647470,
        SEGR = 0x73656772,
        SGPD = 0x73677064,
        SIDX = 0x73696478,
        SINF = 0x73696E66,
        SKIP = 0x736b6970,
        SMHD = 0x736d6864,
        SSIX = 0x73736978,
        STBL = 0x7374626c,
        STCO = 0x7374636f,
        STDP = 0x73746470,
        STHD = 0x73746864,
        STRD = 0x73747264,
        STRI = 0x73747269,
        STRK = 0x7374726B,
        STSC = 0x73747363,
        STSD = 0x73747364,
        STSH = 0x73747368,
        STSS = 0x73747373,
        STSZ = 0x7374737a,
        STTS = 0x73747473,
        STYP = 0x73747970,
        STZ2 = 0x73747a32,
        SUBS = 0x73756273,
        TFDT = 0x74666474,
        TFHD = 0x74666864,
        TFRA = 0x74667261,
        TKHD = 0x746b6864,
        TRAF = 0x74726166,
        TRAK = 0x7472616b,
        TREF = 0x74726566,
        TREX = 0x74726578,
        TRGR = 0x74726772,
        TRUN = 0x7472756E,
        TSEL = 0x7473656C,
        UDTA = 0x75647461,
        UUID = 0x75756964,
        VMHD = 0x766d6864,
        WIDE = 0x77696465,
        XML_ = 0x786D6C20,

        // VisualSampleEntry
        AVCC = 0x61766343, // avcC
        BTRT = 0x62747274,
        CLAP = 0x636c6170,
        COLR = 0x636f6c72,
        PASP = 0x70617370,

        // 6.2.1 ISO/IEC 14496-12:2015
        // Should not be use anymore but was in previous implementation
        CHAP = 0x63686170,
        CLIP = 0x636c6970,
        CRGN = 0x6372676e,
        CTAB = 0x63746162,
        IMAP = 0x696d6170,
        KMAT = 0x6b6d6174,
        LOAD = 0x6c6f6164,
        MATT = 0x6d617474,
        PNOT = 0x706e6f74,
        SCPT = 0x73637074,
        SYNC = 0x73796e63,
        TMCD = 0x746d6364,
    }

    /// <summary>
    /// FourCC brands. The value is the FourCC in decimal
    /// </summary>
    public enum ISOBrand : uint
    {
        AVC1 = 0x61766331,
        ISO2 = 0x69736f32,
        ISO3 = 0x69736f33,
        ISO4 = 0x69736f34,
        ISO5 = 0x69736f35,
        ISO6 = 0x69736f36,
        ISO7 = 0x69736f37,
        ISO8 = 0x69736f38,
        ISO9 = 0x69736f39,
        ISOM = 0x69736f6d,
        MP41 = 0x6d703431,
        MP42 = 0x6d703432,
        MP71 = 0x6d703731,
    }

    public enum ISOHandler
    {
        None = 0,
        VIDE = 0x76696465,
        SOUN = 0x736f756e
    }

    // ISO 639-2 Code
    // https://en.wikipedia.org/wiki/List_of_ISO_639-2_codes
    // padding (1 bit) + character (5 bits)[3]
    public enum ISOLanguage
    {
        ZXX = ('z' - 96) << 10 | ('x' - 96) << 5 | 'x' - 96,
        UND = ('u' - 96) << 10 | ('n' - 96) << 5 | 'd' - 96,
        ENG = ('e' - 96) << 10 | ('n' - 96) << 5 | 'g' - 96,
        MUL = ('m' - 96) << 10 | ('u' - 96) << 5 | 'l' - 96,
        MIS = ('m' - 96) << 10 | ('i' - 96) << 5 | 's' - 96,
        FRA = ('f' - 96) << 10 | ('r' - 96) << 5 | 'a' - 96,
        FRE = ('f' - 96) << 10 | ('r' - 96) << 5 | 'e' - 96,
    }
}
