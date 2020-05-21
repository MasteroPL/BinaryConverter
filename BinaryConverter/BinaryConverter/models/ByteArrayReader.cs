using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryConverter.models
{
	public class ByteArrayReader : IBinaryReadable
	{
		private IEnumerable<byte> SourceArray;
		private IEnumerator<byte> Iterator;
		private int Counter = 0;
		private bool IsEndOfSource;

		public ByteArrayReader(IEnumerable<byte> sourceArray) {
			if(sourceArray == null) {
				throw new ArgumentNullException("Source Array cannot be null");
			}

			SourceArray = sourceArray;
			Iterator = SourceArray.GetEnumerator();
			IsEndOfSource = !Iterator.MoveNext();
		}

		public byte GetNextByte() {
			if (IsEndOfSource) {
				throw new EndOfSourceException("Cannot read beyond end of source array", new IndexOutOfRangeException());
			}

			byte current = Iterator.Current;
			IsEndOfSource = !Iterator.MoveNext();
			return current;
		}

		public bool EndOfSource() {
			return IsEndOfSource;
		}
	}
}
