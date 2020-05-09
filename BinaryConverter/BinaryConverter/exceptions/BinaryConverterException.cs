using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinaryConverter.exceptions
{
	/// <summary>
	/// Base exception class for exceptions strictly concerning BinaryConverter and its utilities
	/// </summary>
	public class BinaryConverterException : Exception
	{
		public BinaryConverterException() : base() { }
		public BinaryConverterException(string message) : base(message) { }
		public BinaryConverterException(string message, Exception inner) : base(message, inner) { }
		public BinaryConverterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
