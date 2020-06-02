using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// Base attribute for attributes dedicated to configuration of converters within BinaryConverter class and its children
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public abstract class BinaryConverterMethodAttribute : Attribute
	{
		public Type ForType { get; private set; }
		public BinaryConverterMethodAttribute(Type forType) {
			ForType = forType;
		}
	}
}
