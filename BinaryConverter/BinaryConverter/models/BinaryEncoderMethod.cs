using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public class BinaryEncoderMethod : BinaryConverterMethod
	{
		public Func<object, IEnumerable<byte>> Encoder;

		public BinaryEncoderMethod(Type forType, Func<object, IEnumerable<byte>> encoder) : base(forType) {
			Encoder = encoder;
		}
	}
}
