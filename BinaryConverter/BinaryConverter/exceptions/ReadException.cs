using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinaryConverter.exceptions
{
	/// <summary>
	/// Base class for exceptions concerning any failed read operations, strictly concerning BinaryConverter
	/// </summary>
	public class ReadException : BinaryConverterException
	{
		public ReadException() : base() { }
		public ReadException(string message) : base(message) { }
		public ReadException(string message, Exception inner) : base(message, inner) { }
		public ReadException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
