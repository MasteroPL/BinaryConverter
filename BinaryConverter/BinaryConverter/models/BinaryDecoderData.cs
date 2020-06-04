using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public class BinaryDecoderData : BinaryConverterData
	{
		public Func<IEnumerable<byte>, object> Decoder;

		public BinaryDecoderData(Func<IEnumerable<byte>, object> decoder, ConverterType converterType = ConverterType.EXCLUSIVE) : base(converterType) {
			Decoder = decoder;
		}
	}
}
