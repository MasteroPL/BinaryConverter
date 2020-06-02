using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// Defines binary encoder for the entire class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
	public class ClassBinaryEncoderAttribute : Attribute {
		public Func<object, IEnumerable<byte>> Encoder { get; protected set; }

		public ClassBinaryEncoderAttribute(Func<object, IEnumerable<byte>> encoder) {
			Encoder = encoder;
		}
	}
}
