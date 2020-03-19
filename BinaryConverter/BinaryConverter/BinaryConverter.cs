using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter
{
	public class BinaryConverter
	{

		#region Encoding
		public byte[] Encode(object? o) {
			var bytes = new List<byte>();



			return bytes.ToArray();
		}

		#endregion

		#region Decoding
		public virtual object Decode(IEnumerable<byte> bytes) {


			return null;
		}

		#endregion

		#region Predefined Encoders
		protected virtual byte[] _PrimitiveTypesEncoder(object? o) {
			return null;
		}
		#endregion

		#region Predefined Decoders

		#endregion
	}
}
