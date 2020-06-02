using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// Defines "7. A default converter defined within the converter class" (for full list of priorities during conversions, visit documention of class "BinaryConverter"
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class BinaryConverterDefaultEncoderMethodAttribute : BinaryConverterMethodAttribute
	{
		public BinaryConverterDefaultEncoderMethodAttribute(Type forType) : base(forType) { }
	}
}
