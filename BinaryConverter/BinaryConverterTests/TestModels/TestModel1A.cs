using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverterTests.TestModels
{
	public class TestModel1A
	{
		private int a, b, c;
		private float d, e, f;

		public TestModel1A(int a, int b, int c, float d, float e, float f) {
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
			this.e = e;
			this.f = f;
		}

		public virtual bool Validate(TestModel1A o) {
			return this.a == o.a && this.b == o.b && this.c == o.c && this.d == o.d && this.e == o.e && this.f == o.f;
		}
	}
}
