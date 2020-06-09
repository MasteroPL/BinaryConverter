using BinaryConverterTests.TestModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	}
}
