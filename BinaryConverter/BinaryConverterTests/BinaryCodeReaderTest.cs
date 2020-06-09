using BinaryConverter.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverterTests
{
	[TestClass]
	public class BinaryCodeReaderTest
	{
		[TestMethod]
		public void TestReadBytes() {
			var bytes = new byte[] { 113, 25, 45, 79, 13, 51 };
			var reader = new BinaryCodeReader(bytes);

			for(int i = 0; i < bytes.Length; i++) {
				Assert.AreEqual(bytes[i], reader.ReadNextByte());
			}
		}
		[TestMethod]
		public void TestReadBits() {
			var bytes = new byte[] { 113, 11, 51 };
			var bits = new byte[] { 
				1, 0, 0, 0, 1, 1, 1, 0,
				1, 1, 0, 1, 0, 0, 0, 0,
				1, 1, 0, 0, 1, 1, 0, 0
			};

			var reader = new BinaryCodeReader(bytes);

			for(int i = 0; i < bits.Length; i++) {
				Assert.AreEqual(bits[i], reader.ReadNextBit());
			}
		}

		[TestMethod]
		public void TestReadAfterMove() {
			var bytes = new byte[] { 117, 243, 11, 3, 57 };
			var reader = new BinaryCodeReader(bytes);

			reader.MoveBytePointer(1);
			Assert.AreEqual(bytes[1], reader.ReadNextByte());
			reader.MoveBytePointer(-1);
			reader.MoveBitPointer(3);
			Assert.AreEqual(126, reader.ReadNextByte());
			reader.MoveBitPointer(5);
			Assert.AreEqual(3, reader.ReadNextByte());
			reader.MoveBytePointer(-4);
			Assert.AreEqual(117, reader.ReadNextByte());
		}
	}
}
