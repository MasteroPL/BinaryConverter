using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public abstract class BinaryConverterMethod
	{
		public Type ForType;

		public BinaryConverterMethod(Type forType) {
			ForType = forType;
		}
	}
}
