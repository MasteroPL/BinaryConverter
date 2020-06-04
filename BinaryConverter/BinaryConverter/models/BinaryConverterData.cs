using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	/// <summary>
	/// There is only 1 purpose of this class, namely to give the encoder the information whether the converter type is inclusive or exclusive (inclusive is only for serializable types)
	/// </summary>
	public abstract class BinaryConverterData
	{
		public ConverterType ConverterType;

		public BinaryConverterData(ConverterType converterType) {
			ConverterType = converterType;
		}
	}
}
