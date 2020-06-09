using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverterTests.TestModels
{
	public class TestModel1D : TestModel1C
	{
		public long h = 0;

		public TestModel1D(int a, int b, int c, float d, float e, float f, string text, double g) : base(a, b, c, d, e, f, text, g) { }

		public override bool Validate(TestModel1A o) {
			TestModel1D obj = (TestModel1D)o;
			return base.Validate(o) && this.h == obj.h;
		}
	}
}
