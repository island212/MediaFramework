using MediaFramework.LowLevel.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unsafe
{
    public class BBitReaderTests
    {
        private const int StreamSize = 16;

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
        public void Constructor_ValidBByteReader_ValidState()
        {
            Stream.Add(0x59);
            Stream.Add(0xF9);
            Stream.Add(0xC3);

            var bbyteReader = new BByteReader(Stream);

            var bitReader = new BBitReader(bbyteReader);

            unsafe
            {
                Assert.IsTrue(bitReader.m_Buffer == bbyteReader.m_Head, "Buffer");
                Assert.AreEqual(0, bitReader.Index, "Index");
                Assert.AreEqual(24, bitReader.Length, "Length");
                Assert.IsTrue(bitReader.IsValid, "IsValid");
            }
        }

        [Test]
        public void Constructor_InvalidBByteReader_InvalidState()
        {
            var bbyteReader = new BByteReader();

            var bitReader = new BBitReader(bbyteReader);

            unsafe
            {
                Assert.IsTrue(bitReader.m_Buffer == null, "Buffer");
                Assert.AreEqual(0, bitReader.Index, "Index");
                Assert.AreEqual(0, bitReader.Length, "Length");
                Assert.IsFalse(bitReader.IsValid, "IsValid");
            }
        }

        [Test]
        public void Constructor_ValidNativeList_ValidState()
        {
            Stream.Add(0x59);
            Stream.Add(0xF9);
            Stream.Add(0xC3);

            var bitReader = new BBitReader(Stream);

            unsafe
            {
                Assert.IsTrue(bitReader.m_Buffer == Stream.GetUnsafeReadOnlyPtr(), "Buffer");
                Assert.AreEqual(0, bitReader.Index, "Index");
                Assert.AreEqual(24, bitReader.Length, "Length");
                Assert.IsTrue(bitReader.IsValid, "IsValid");
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

                var bitReader = new BBitReader(ptr, Stream.Length);

                Assert.IsTrue(bitReader.m_Buffer == ptr, "Buffer");
                Assert.AreEqual(0, bitReader.Index, "Index");
                Assert.AreEqual(24, bitReader.Length, "Length");
                Assert.IsTrue(bitReader.IsValid, "IsValid");
            }
        }

        [Test]
        public void Constructor_InvalidPtrAndLength_InvalidState()
        {
            unsafe
            {
                var bitReader = new BBitReader(null, -1);

                Assert.IsTrue(bitReader.m_Buffer == null, "Buffer");
                Assert.AreEqual(0, bitReader.Index, "Index");
                Assert.AreEqual(-8, bitReader.Length, "Length");
                Assert.IsFalse(bitReader.IsValid, "IsValid");
            }
        }

        [Test]
        public void HasEnoughBits_EqualLength_ReturnTrue()
        {
            Stream.Add(0x59);
            Stream.Add(0xF9);
            Stream.Add(0xC3);

            var bitReader = new BBitReader(Stream);

            Assert.IsTrue(bitReader.HasEnoughBits(24), "HasEnoughBits");
        }

        [Test]
        public void HasEnoughBits_LessThanLength_ReturnTrue()
        {
            Stream.Add(0x59);
            Stream.Add(0xF9);
            Stream.Add(0xC3);

            var bitReader = new BBitReader(Stream);

            Assert.IsTrue(bitReader.HasEnoughBits(23), "HasEnoughBits");
        }

        [Test]
        public void HasEnoughBits_GreaterThanLength_ReturnFalse()
        {
            Stream.Add(0x59);
            Stream.Add(0xF9);
            Stream.Add(0xC3);

            var bitReader = new BBitReader(Stream);

            Assert.IsFalse(bitReader.HasEnoughBits(25), "HasEnoughBits");
        }

        [Test]
        public void ReadBitWithoutCheck_ReadAllBuffer_Return0_0_0_0_1_0_1_1()
        {
            Stream.Add(0x0B);

            var bitReader = new BBitReader(Stream);

            Assert.AreEqual(0, bitReader.ReadBitWithoutCheck(), "Bit 1");
            Assert.AreEqual(0, bitReader.ReadBitWithoutCheck(), "Bit 2");
            Assert.AreEqual(0, bitReader.ReadBitWithoutCheck(), "Bit 3");
            Assert.AreEqual(0, bitReader.ReadBitWithoutCheck(), "Bit 4");
            Assert.AreEqual(1, bitReader.ReadBitWithoutCheck(), "Bit 5");
            Assert.AreEqual(0, bitReader.ReadBitWithoutCheck(), "Bit 6");
            Assert.AreEqual(1, bitReader.ReadBitWithoutCheck(), "Bit 7");
            Assert.AreEqual(1, bitReader.ReadBitWithoutCheck(), "Bit 8");
        }

        [Test]
        public void TryReadBits_ReadAllBufferOneBit_Return0_0_0_0_1_0_1_1()
        {
            Stream.Add(0x0B);

            var bitReader = new BBitReader(Stream);

            uint bit;
            ReaderError error;

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 1 error");
            Assert.AreEqual(0, bit, "Bit 1");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 2 error");
            Assert.AreEqual(0, bit, "Bit 2");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 3 error");
            Assert.AreEqual(0, bit, "Bit 3");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 4 error");
            Assert.AreEqual(0, bit, "Bit 4");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 5 error");
            Assert.AreEqual(1, bit, "Bit 5");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 6 error");
            Assert.AreEqual(0, bit, "Bit 6");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 7 error");
            Assert.AreEqual(1, bit, "Bit 7");

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "Bit 8 error");
            Assert.AreEqual(1, bit, "Bit 8");
        }

        [Test]
        public void TryReadBits_ReadAllBufferInUnevenNumber_Return1_1_11_59()
        {
            Stream.Add(0x95);
            Stream.Add(0xBB);

            var bitReader = new BBitReader(Stream);

            uint bit;
            ReaderError error;

            error = bitReader.TryReadBits(1, out bit);
            Assert.AreEqual(ReaderError.None, error, "1 Bit error");
            Assert.AreEqual(1, bit, "1 Bit");

            error = bitReader.TryReadBits(3, out bit);
            Assert.AreEqual(ReaderError.None, error, "3 Bits error");
            Assert.AreEqual(1, bit, "3 Bits");

            error = bitReader.TryReadBits(5, out bit);
            Assert.AreEqual(ReaderError.None, error, "5 Bits error");
            Assert.AreEqual(11, bit, "5 Bits");

            error = bitReader.TryReadBits(7, out bit);
            Assert.AreEqual(ReaderError.None, error, "7 Bits error");
            Assert.AreEqual(59, bit, "7 Bits");
        }

        [Test]
        public void TryReadBits_ReadOverBufferLength_ReturnError()
        {
            Stream.Add(0x95);
            Stream.Add(0xBB);

            var bitReader = new BBitReader(Stream);

            uint bit;
            ReaderError error;

            error = bitReader.TryReadBits(17, out bit);
            Assert.AreEqual(ReaderError.OutOfRange, error, "Error");
        }

        [Test]
        public void TryReadBits_ReadZeroBit_Return0()
        {
            Stream.Add(0x0B);

            var bitReader = new BBitReader(Stream);

            uint bit;
            ReaderError error;

            error = bitReader.TryReadBits(0, out bit);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(0, bit, "Value");
        }

        [Test]
        public void TryReadBool_ReadOne_ReturnTrue()
        {
            Stream.Add(0x80);

            var bitReader = new BBitReader(Stream);

            bool condition;
            ReaderError error;

            error = bitReader.TryReadBool(out condition);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(true, condition, "Value");
        }

        [Test]
        public void TryReadBool_ReadOne_ReturnFalse()
        {
            Stream.Add(0x00);

            var bitReader = new BBitReader(Stream);

            bool condition;
            ReaderError error;

            error = bitReader.TryReadBool(out condition);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(false, condition, "Value");
        }

        [Test]
        public void TryReadBool_ReadOverBufferLength_ReturnError()
        {
            Stream.Add(0x00);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 8;

            bool condition;
            ReaderError error;

            error = bitReader.TryReadBool(out condition);
            Assert.AreEqual(ReaderError.OutOfRange, error, "Error");
        }

        [Test]
        public void TryReadUExpGolomb_ReadOne_Return4()
        {
            Stream.Add(0x28);

            var bitReader = new BBitReader(Stream);

            uint value;
            ReaderError error;

            error = bitReader.TryReadUExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(4, value, "Value");
        }

        [Test]
        public void TryReadUExpGolomb_ReadOne_Return0()
        {
            Stream.Add(0x80);

            var bitReader = new BBitReader(Stream);

            uint value;
            ReaderError error;

            error = bitReader.TryReadUExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(0, value, "Value");
        }

        [Test]
        public void TryReadUExpGolomb_ReadOneLongerThan32Bits_ReturnOverflow()
        {
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x01);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 7;

            uint value;
            ReaderError error;

            error = bitReader.TryReadUExpGolomb(out value);
            Assert.AreEqual(ReaderError.Overflow, error, "Error");
        }

        [Test]
        public void TryReadUExpGolomb_ReadOneNoBitLeft_ReturnOutOfRange()
        {
            Stream.Add(0x00);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 8;

            uint value;
            ReaderError error;

            error = bitReader.TryReadUExpGolomb(out value);
            Assert.AreEqual(ReaderError.OutOfRange, error, "Error");
        }

        [Test]
        public void TryReadUExpGolomb_ReadOneWithoutEnoughBit_ReturnOutOfRange()
        {
            Stream.Add(0x00);
            Stream.Add(0x40);

            var bitReader = new BBitReader(Stream);

            uint value;
            ReaderError error;

            error = bitReader.TryReadUExpGolomb(out value);
            Assert.AreEqual(ReaderError.OutOfRange, error, "Error");
        }

        [Test]
        public void TryReadUExpGolomb_ReadOneWithEnoughBit_Return7()
        {
            Stream.Add(0x00);
            Stream.Add(0x80);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 1;

            uint value;
            ReaderError error;

            error = bitReader.TryReadUExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(127, value, "Value");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOne_ReturnMinus2()
        {
            Stream.Add(0x28);

            var bitReader = new BBitReader(Stream);

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(-2, value, "Value");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOne_Return2()
        {
            Stream.Add(0x20);

            var bitReader = new BBitReader(Stream);

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(2, value, "Value");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOne_Return0()
        {
            Stream.Add(0x80);

            var bitReader = new BBitReader(Stream);

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(0, value, "Value");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOneLongerThan32Bits_ReturnOverflow()
        {
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x01);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);
            Stream.Add(0x00);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 7;

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.Overflow, error, "Error");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOneNoBitLeft_ReturnOutOfRange()
        {
            Stream.Add(0x00);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 8;

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.OutOfRange, error, "Error");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOneWithoutEnoughBit_ReturnOutOfRange()
        {
            Stream.Add(0x00);
            Stream.Add(0x40);

            var bitReader = new BBitReader(Stream);

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.OutOfRange, error, "Error");
        }

        [Test]
        public void TryReadSExpGolomb_ReadOneWithEnoughBit_Return7()
        {
            Stream.Add(0x00);
            Stream.Add(0x80);

            var bitReader = new BBitReader(Stream);

            bitReader.m_Index = 1;

            int value;
            ReaderError error;

            error = bitReader.TryReadSExpGolomb(out value);
            Assert.AreEqual(ReaderError.None, error, "Error");
            Assert.AreEqual(64, value, "Value");
        }
    }
}