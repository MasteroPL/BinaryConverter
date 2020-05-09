using System;
using System.Collections.Generic;
using System.Text;

using BinaryConverter.models;

namespace BinaryConverter.utils
{
	public class BinaryCodeReader
	{
		private IBinaryReadable Source;
		private byte BitPointer = 0;
		private byte CurrentByte;

		public BinaryCodeReader(IBinaryReadable source) {
			if (source == null) {
				throw new ArgumentNullException("IBinaryReadable cannot be a null instance");
			}

			Source = source;
		}
		public BinaryCodeReader(IEnumerable<byte> source) {
			if (source == null) {
				throw new ArgumentNullException("Source cannot be a null instance");
			}

			Source = new ByteArrayReadable(source);
		}
		/// <summary>
		/// Reads next single bit from the source
		/// </summary>
		/// <returns>A value of that single bit (either 1 or 0)</returns>
		public byte ReadNextBit() {
			byte result;
			int conjuctor = 1; // for the sake of not converting to byte

			// Special case, requires reading the next byte from source
			if(BitPointer == 0) {
				CurrentByte = Source.GetNextByte();
			}

			conjuctor = (conjuctor << BitPointer);
			result = (byte)((CurrentByte & conjuctor) >> BitPointer);

			return result;
		}
		/// <summary>
		/// Reads a number of next bits from the source
		/// </summary>
		/// <param name="numberOfBits">Number of bits to read from the source</param>
		/// <returns>Array of bits from the source</returns>
		public byte[] ReadNextBits(uint numberOfBits) {
			if(numberOfBits < 1) {
				throw new ArgumentException("Number of bits has to be at least 1");
			}

			byte[] result = new byte[numberOfBits];

			for(int i = 0; i < numberOfBits; i++) {
				result[i] = ReadNextBit();
			}

			return result;
		}

		public byte ReadNextByte() {
			// Special case that requires us to do absolutely nothing
			if(BitPointer == 0) {
				CurrentByte = Source.GetNextByte();
				return CurrentByte;
			}

			byte result = (byte)(CurrentByte >> BitPointer);
			CurrentByte = Source.GetNextByte();
			result += (byte)(CurrentByte << (8 - BitPointer));

			return result;
		}

		public byte[] ReadNextBytes(uint numberOfBytes) {
			if (numberOfBytes < 1) {
				throw new ArgumentException("Number of bytes has to be at least 1");
			}

			byte[] result = new byte[numberOfBytes];

			for(int i = 0; i < numberOfBytes; i++) {
				result[i] = ReadNextByte();
			}

			return result;
		}

		public bool EndOfSource() {
			return Source.EndOfSource();
		}
	}
}
