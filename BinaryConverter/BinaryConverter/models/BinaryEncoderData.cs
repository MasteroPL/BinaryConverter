using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public class BinaryEncoderData : BinaryConverterData
	{
		public Func<object, IEnumerable<byte>> Encoder;

		public BinaryEncoderData(Func<object, IEnumerable<byte>> encoder, ConverterType converterType = ConverterType.EXCLUSIVE) : base(converterType) {
			Encoder = encoder;
		}
	}
}
