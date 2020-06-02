using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public class BinaryDecoderMethod : BinaryConverterMethod
	{
		public Func<IEnumerable<byte>, object> Decoder;

		public BinaryDecoderMethod(Type forType, Func<IEnumerable<byte>, object> decoder) : base(forType) {
			Decoder = decoder;
		}
	}
}
