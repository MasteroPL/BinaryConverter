using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace BinaryConverter.exceptions
{
	public class ConverterMethodException : BinaryConverterException
	{
		public object ErrorForObject;
		public MethodInfo ErrorForMethod;

		public ConverterMethodException(object errorForObject, MethodInfo errorForMethod) : base() {
			ErrorForObject = errorForObject;
			ErrorForMethod = errorForMethod;
		}
		public ConverterMethodException(object errorForObject, MethodInfo errorForMethod, string message) : base(message) {
			ErrorForObject = errorForObject;
			ErrorForMethod = errorForMethod;
		}
		public ConverterMethodException(object errorForObject, MethodInfo errorForMethod, string message, Exception inner) : base(message, inner) {
			ErrorForObject = errorForObject;
			ErrorForMethod = errorForMethod;
		}
		public ConverterMethodException(object errorForObject, MethodInfo errorForMethod, SerializationInfo info, StreamingContext context) : base(info, context) {
			ErrorForObject = errorForObject;
			ErrorForMethod = errorForMethod;
		}
	}
}
