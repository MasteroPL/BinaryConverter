using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	/// <summary>
	/// Defines type of the converter
	/// </summary>
	public enum ConverterType
	{
		/// <summary>
		/// (Non-serializable object conversion) Exclusive means, the converter returns only the binary representation of an object, without class type meta data
		/// </summary>
		EXCLUSIVE = 0,
		/// <summary>
		/// (Serializable object conversion) Inclusive means, the converter returns binary representation of an object with class type meta data included
		/// </summary>
		INCLUSIVE_SERIALIZABLE = 1,
		/// <summary>
		/// (Primitive object conversion) Inclusive means, the converter returns binary representation of an object with class type meta data included
		/// </summary>
		INCLUSIVE_PRIMITIVE = 2
	}
}
