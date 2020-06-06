using System;
using System.Runtime.Serialization;

namespace BinaryConverter.exceptions
{
	public class InfiniteEncodingLoopException : BinaryConverterException
	{
		public object ForObject;

		public InfiniteEncodingLoopException(object forObject) : base() {
			ForObject = forObject;
		}
		public InfiniteEncodingLoopException(object forObject, string message) : base(message) {
			ForObject = forObject;
		}
		public InfiniteEncodingLoopException(object forObject, string message, Exception inner) : base(message, inner) {
			ForObject = forObject;
		}
		public InfiniteEncodingLoopException(object forObject, SerializationInfo info, StreamingContext context) : base(info, context) {
			ForObject = forObject;
		}
	}
}
