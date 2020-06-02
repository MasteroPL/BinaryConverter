using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// Attribute used for marking a method that should be used for binary encoding for a class it's defined in
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class CustomEncoderMethodAttribute : Attribute {}
}
