using BinaryConverterTests.TestModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinaryConverter.exceptions;

namespace BinaryConverterTests
{
	[TestClass]
	public class BinaryConverterTest
	{
		[TestMethod]
		public void TestConverting() {
			var testObject = new TestModel1D(5, 10, 15, 0.5f, 0.13f, 0.27f, "tekst", 0.22);
			testObject.h = 120432;

			var converter = new BinaryConverter.models.BinaryConverter();
			var bytes = converter.Encode(testObject);
			TestModel1D decoded = (TestModel1D)converter.Decode(bytes.ToArray());

			// Compares testObject with decoded
			Assert.IsTrue(testObject.Validate(decoded));
		}

		[TestMethod]
		public void TestComplexConverting() {
			var testObject = new TestModel2();
			testObject.Parent = new TestModel2();

			var converter = new BinaryConverter.models.BinaryConverter();
			var bytes = converter.Encode(testObject);
			TestModel2 decoded = (TestModel2)converter.Decode(bytes.ToArray());

			Assert.IsTrue(decoded.Validate(testObject));
		}

		[TestMethod]
		public void TestRecursiveLoop() {
			var testObject = new TestModel2();
			testObject.Parent = testObject;
			var converter = new BinaryConverter.models.BinaryConverter();

			try {
				converter.Encode(testObject);
				Assert.IsFalse(true, "An InfiniteEncodingLoopException should have been thrown"); // exception not thrown - error
			} catch(InfiniteEncodingLoopException e) {
				Assert.IsTrue(true, "InfiniteEncodingLoopException was thrown - as it should have been"); //  exception thrown - as it should have been
			}
		}
	}
}
