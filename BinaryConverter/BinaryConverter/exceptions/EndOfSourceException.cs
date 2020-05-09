using BinaryConverter.exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinaryConverter
{
	public class EndOfSourceException : ReadException
	{
		public EndOfSourceException() : base() { }
		public EndOfSourceException(string message) : base(message) { }
		public EndOfSourceException(string message, Exception inner) : base(message, inner) { }
		public EndOfSourceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
