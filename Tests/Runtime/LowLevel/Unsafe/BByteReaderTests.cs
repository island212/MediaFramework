using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using MediaFramework.LowLevel.Unsafe;

public class BByteReaderTests
{
    private const int StreamSize = 1024;

    private NativeList<byte> Stream;

    [SetUp]
    public void SetUp()
    {
        Stream = new NativeList<byte>(StreamSize, Allocator.Temp);
    }

    [TearDown]
    public void TearDown()
    {
        Stream.Clear();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Stream.Dispose();
    }

    [Test]
    public void Constructor_ValidNativeList_ValidState()
    {
        Stream.Add(0x59);
        Stream.Add(0xF9);
        Stream.Add(0xC3);

        var byteReader = new BByteReader(Stream);

        unsafe
        {
            Assert.IsTrue(byteReader.m_Buffer == Stream.GetUnsafeReadOnlyPtr(), "Buffer");
            Assert.AreEqual(0, byteReader.Index, "Index");
            Assert.AreEqual(3, byteReader.Length, "Length");
            Assert.IsTrue(byteReader.IsValid, "IsValid");
        }
    }

    [Test]
    public void Constructor_ValidPtrAndLength_ValidState()
    {
        Stream.Add(0x59);
        Stream.Add(0xF9);
        Stream.Add(0xC3);

        unsafe
        {
            var ptr = (byte*)Stream.GetUnsafeReadOnlyPtr();

            var byteReader = new BByteReader(ptr, Stream.Length);

            Assert.IsTrue(byteReader.m_Buffer == ptr, "Buffer");
            Assert.AreEqual(0, byteReader.Index, "Index");
            Assert.AreEqual(3, byteReader.Length, "Length");
            Assert.IsTrue(byteReader.IsValid, "IsValid");
        }
    }

    [Test]
    public void Constructor_InvalidPtrAndLength_InvalidState()
    {
        unsafe
        {
            var byteReader = new BByteReader(null, -1);

            Assert.IsTrue(byteReader.m_Buffer == null, "Buffer");
            Assert.AreEqual(0, byteReader.Index, "Index");
            Assert.AreEqual(-1, byteReader.Length, "Length");
            Assert.IsFalse(byteReader.IsValid, "IsValid");
        }
    }

    [Test]
    public void ReadUInt8_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x59);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x59u, byteReader.ReadUInt8());
    }

    [Test]
    public void ReadUInt8_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xD9u, byteReader.ReadUInt8());
    }

    [Test]
    public void ReadUInt8_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt8());
    }

    [Test]
    public void ReadInt8_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x59);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x59, byteReader.ReadInt8());
    }

    [Test]
    public void ReadInt8_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xD9 - (1 << 8), byteReader.ReadInt8());
    }

    [Test]
    public void ReadInt8_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadInt8());
    }

    [Test]
    public void ReadUInt16_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9u, byteReader.ReadUInt16());
    }

    [Test]
    public void ReadUInt16_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xEED9u, byteReader.ReadUInt16());
    }

    [Test]
    public void ReadUInt16_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt16());
    }

    [Test]
    public void ReadInt16_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9, byteReader.ReadInt16());
    }

    [Test]
    public void ReadInt16_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xEED9 - (1 << 16), byteReader.ReadInt16());
    }

    [Test]
    public void ReadInt16_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadInt16());
    }

    [Test]
    public void ReadUInt24_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);
        Stream.Add(0xDB);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9DBu, byteReader.ReadUInt24());
    }

    [Test]
    public void ReadUInt24_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);
        Stream.Add(0xDB);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xEED9DBu, byteReader.ReadUInt24());
    }

    [Test]
    public void ReadUInt24_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt24());
    }

    [Test]
    public void ReadInt24_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);
        Stream.Add(0xDB);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9DB, byteReader.ReadInt24());
    }

    [Test]
    public void ReadInt24_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);
        Stream.Add(0xDB);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xEED9DB, byteReader.ReadInt24());
    }

    [Test]
    public void ReadInt24_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadInt24());
    }

    [Test]
    public void ReadUInt32_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9DB58u, byteReader.ReadUInt32());
    }

    [Test]
    public void ReadUInt32_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xEED9DB58u, byteReader.ReadUInt32());
    }

    [Test]
    public void ReadUInt32_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt32());
    }

    [Test]
    public void ReadInt32_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9DB58, byteReader.ReadInt32());
    }

    [Test]
    public void ReadInt32_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(-~0xEED9DB58 - 1, byteReader.ReadInt32());
    }

    [Test]
    public void ReadInt32_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadInt32());
    }


    [Test]
    public void ReadUInt64_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);
        Stream.Add(0x9D);
        Stream.Add(0xBD);
        Stream.Add(0x85);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9DB58E69DBD85UL, byteReader.ReadUInt64());
    }

    [Test]
    public void ReadUInt64_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);
        Stream.Add(0x9D);
        Stream.Add(0xBD);
        Stream.Add(0x85);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xEED9DB58E69DBD85UL, byteReader.ReadUInt64());
    }

    [Test]
    public void ReadUInt64_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);
        Stream.Add(0x9D);
        Stream.Add(0xBD);
        Stream.Add(0x85);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt64());
    }

    [Test]
    public void ReadInt64_SinglePositive_ReadCorrectValue()
    {
        Stream.Add(0x6E);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);
        Stream.Add(0x9D);
        Stream.Add(0xBD);
        Stream.Add(0x85);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0x6ED9DB58E69DBD85L, byteReader.ReadInt64());
    }

    [Test]
    public void ReadInt64_SingleNegative_ReadCorrectValue()
    {
        Stream.Add(0xEE);
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);
        Stream.Add(0x9D);
        Stream.Add(0xBD);
        Stream.Add(0x85);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(unchecked((long)0xEED9DB58E69DBD85L), byteReader.ReadInt64());
    }

    [Test]
    public void ReadInt64_OneMissingByte_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);
        Stream.Add(0x9D);
        Stream.Add(0xBD);
        Stream.Add(0x85);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadInt64());
    }

    [Test]
    public void Index_NegativeIndex_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => { byteReader.Index = -1; });
    }

    [Test]
    public void Index_OneByteOverflow_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => { byteReader.Index = 5; });
    }

    [Test]
    public void Index_JumpToZero_ReadCorrectValue()
    {
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        Assert.AreEqual(0xD9DB58E6u, byteReader.ReadUInt32());
        Assert.AreEqual(4, byteReader.Index);

        byteReader.Index = 0;

        Assert.AreEqual(0, byteReader.Index);
        Assert.AreEqual(0xD9DB58E6u, byteReader.ReadUInt32());
    }

    [Test]
    public void Index_JumpToMiddle_ReadCorrectValue()
    {
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.Index = 2;

        Assert.AreEqual(0x58, byteReader.ReadUInt8());
        Assert.AreEqual(0xE6, byteReader.ReadUInt8());
    }

    [Test]
    public void Index_JumpToEnd_ExpectNoException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.Index = 4;
    }

    [Test]
    public void Index_JumpToEndReadOneByte_ExpectException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.Index = 4;

        Assert.AreEqual(4, byteReader.Index);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt8());
    }

    [Test]
    public void Seek_NegativeIndex_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => { byteReader.Seek(-1); });
    }

    [Test]
    public void Seek_OneByteOverflow_ThrowException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        Assert.Throws<ArgumentOutOfRangeException>(() => { byteReader.Seek(5); });
    }

    [Test]
    public void Seek_JumpToZero_ReadCorrectValue()
    {
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.ReadUInt32();

        Assert.AreEqual(4, byteReader.Index);

        byteReader.Seek(-4);

        Assert.AreEqual(0, byteReader.Index);
    }

    [Test]
    public void Seek_JumpToMiddle_ReadCorrectValue()
    {
        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.Seek(2);

        Assert.AreEqual(2, byteReader.Index);
    }

    [Test]
    public void Seek_JumpToEnd_ExpectNoException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.Seek(4);

        Assert.AreEqual(4, byteReader.Index);
    }

    [Test]
    public void Seek_JumpToEndReadOneByte_ExpectException()
    {
#if !ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.Ignore();
#endif

        Stream.Add(0xD9);
        Stream.Add(0xDB);
        Stream.Add(0x58);
        Stream.Add(0xE6);

        var byteReader = new BByteReader(Stream);

        byteReader.Seek(4);

        Assert.AreEqual(4, byteReader.Index);

        Assert.Throws<ArgumentOutOfRangeException>(() => byteReader.ReadUInt8());
    }
}
