using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// Extension to CustomDecoderMethod designed to be used within BinaryConverter class
	/// 
	/// Defines "3. A custom converter defined within the converter class" (for full list of priorities during conversions, visit documention of class "BinaryConverter"
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class BinaryConverterCustomDecoderMethodAttribute : BinaryConverterMethodAttribute
	{
		public BinaryConverterCustomDecoderMethodAttribute(Type forType) : base(forType) {}
	}
}
