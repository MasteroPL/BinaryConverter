using System;

using BinaryConverter.utils;

namespace DebugConsole
{
	class Program
	{
		static void Main(string[] args) {
			var storer = new BinaryCodeBuilder();

			var a = new byte[] {
				0, 1, 1, 0, 0
			};
			var b = new byte[] {
				1, 1, 1, 0, 1
			};
			var c = new byte[] {
				0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1
			};
			storer.AppendBits(a);
			storer.AppendBits(b);
			storer.AppendBits(c);

			storer.Print();
		}
	}
}
