using BinaryConverter.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverterTests
{
	[TestClass]
	public class BinaryCodeBuilderTest
	{
		[TestMethod]
		public void TestBytesAdd() {
			var builder1 = new BinaryCodeBuilder();
			var builder2 = new BinaryCodeBuilder();

			var bytes = new byte[] { 100, 140, 255, 0, 24 };

			for(int i = 0; i < bytes.Length; i++) {
				builder1.AppendByte(bytes[i]);
			}
			builder2.AppendBytes(bytes);

			var storedBytes1 = builder1.ToBytes();
			var storedBytes2 = builder2.ToBytes();

			for(int i = 0; i < bytes.Length; i++) {
				Assert.AreEqual(bytes[i], storedBytes1[i]);
				Assert.AreEqual(bytes[i], storedBytes2[i]);
			}
		}
		
		[TestMethod]
		public void TestBitsAdd() {
			var builder1 = new BinaryCodeBuilder();
			var builder2 = new BinaryCodeBuilder();

			var bytes = new byte[] { 100, 255, 24 };
			var bits = new byte[] { 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0 };

			for(int i = 0; i < bits.Length; i++) {
				builder1.AppendBit(bits[i]);
			}
			builder2.AppendBits(bits);

			var storedBytes1 = builder1.ToBytes();
			var storedBytes2 = builder2.ToBytes();

			for (int i = 0; i < bytes.Length; i++) {
				Assert.AreEqual(bytes[i], storedBytes1[i]);
				Assert.AreEqual(bytes[i], storedBytes2[i]);
			}
		}

		[TestMethod]
		public void TestBuilderAdd() {
			var builder1 = new BinaryCodeBuilder(
				new byte[] { 100, 255 }
			);
			var builder2 = new BinaryCodeBuilder();

			var bytes = new byte[] { 100, 255, 201 };

			builder1.AppendBit(1);
			builder1.AppendBit(0);
			builder1.AppendBit(0);
			builder1.AppendBit(1);
			builder1.AppendBit(0);

			builder2.Append(builder1);
			builder2.AppendBit(0);
			builder2.AppendBit(1);
			builder2.AppendBit(1);

			var storedBytes2 = builder2.ToBytes();

			for (int i = 0; i < bytes.Length; i++) {
				Assert.AreEqual(bytes[i], storedBytes2[i]);
			}
		}
	}
}
