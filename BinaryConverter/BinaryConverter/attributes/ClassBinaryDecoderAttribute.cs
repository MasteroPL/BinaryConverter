﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// Defines binary decoder for the entire class
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class ClassBinaryDecoderAttribute : Attribute
	{
		public Func<IEnumerable<byte>, object> Decoder { get; protected set; }

		public ClassBinaryDecoderAttribute(Func<IEnumerable<byte>, object> decoder) {
			Decoder = decoder;
		}
	}
}
