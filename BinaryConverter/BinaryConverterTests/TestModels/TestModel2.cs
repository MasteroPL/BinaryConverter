using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverterTests.TestModels
{
	public class TestModel2
	{
		public int Integer = 15;
		private List<int> IntegerList = new List<int>(new int[] { 1, 2, 3 });
		private string Text = "Some random text";
		private TestModel1D AnotherClassObject = new TestModel1D(15, 14, 13, 12, 11, 10, "Hello World!", 9) {
			parent = new TestModel1B(10, 15, 3, 4, 10, 12, "Hello World2!", 155),
			h = 14
		};

		public TestModel2 Parent = null;

		public bool Validate(TestModel2 o) {
			if (Integer != o.Integer) return false;
			if (IntegerList.Count != o.IntegerList.Count) return false;

			for(int i = 0; i < IntegerList.Count; i++) {
				if (IntegerList[i] != o.IntegerList[i]) return false;
			}

			if (!(Text.CompareTo(o.Text) == 0)) return false;

			if (Parent != null) {
				if (o.Parent == null) return false;

				if (!Parent.Validate(o.Parent)) return false;
			}
			else if (o.Parent != null) return false;

			if (!AnotherClassObject.Validate(o.AnotherClassObject)) return false;

			return true;
		}
	}
}
