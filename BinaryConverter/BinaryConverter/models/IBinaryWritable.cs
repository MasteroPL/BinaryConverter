using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	interface IBinaryWritable
	{
		void WriteByte(byte b);
		void WriteBytes(IEnumerable<byte> b);
	}
}
