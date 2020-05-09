using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public interface IBinaryReadable
	{
		byte GetNextByte();
		bool EndOfSource();
	}
}
