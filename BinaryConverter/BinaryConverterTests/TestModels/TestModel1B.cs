using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverterTests.TestModels
{
	public class TestModel1B : TestModel1A
	{
		protected string text;
		public TestModel1B parent = null;
		private double g;

		public TestModel1B(int a, int b, int c, float d, float e, float f, string text, double g) : base(a, b, c, d, e, f) {
			this.text = text;
			this.g = g;
		}

		public override bool Validate(TestModel1A o) {
			var obj = (TestModel1B)o;
			return base.Validate(o) && this.g == obj.g && string.Compare(this.text, obj.text) == 0;
		}
	}
}
