using BinaryConverter.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryConverter.utils
{
	/// <summary>
	/// Similarly to StringBuilder, you can use it to create a binary chain bit by bit, byte by byte or mixing the two
	/// </summary>
	public class BinaryCodeBuilder
	{
		/// <summary>
		/// Points at the current bit position in the current byte (takes values only from range 0-7)
		/// </summary>
		private byte BitPointer = 0;
		/// <summary>
		/// Stored bytes
		/// </summary>
		private List<byte> Bytes = new List<byte>();

		public BinaryCodeBuilder(IEnumerable<byte> initialBytesSet = null) {
			if (initialBytesSet != null) {
				Bytes.AddRange(initialBytesSet);
			}
		}

		/// <summary>
		/// Adds a single bit to the chain
		/// </summary>
		/// <param name="bit">Single bit value</param>
		public void AppendBit(byte bit) {
			if(bit > 1) {
				throw new ArgumentOutOfRangeException("bit argument can only take values of 0 and 1");
			}

			if(BitPointer == 0) {
				Bytes.Add(bit);
			}
			else if(bit == 1) {
				Bytes[Bytes.Count - 1] += (byte)(1 << BitPointer);
			}

			BitPointer++;
			if (BitPointer == 8) BitPointer = 0;
		}
		/// <summary>
		/// Adds a series of bits to the chain
		/// </summary>
		/// <param name="bits">List of bits to add</param>
		public void AppendBits(IEnumerable<byte> bits) {
			foreach(byte b in bits) {
				AppendBit(b);
			}
		}
		/// <summary>
		/// Adds 8 bits to the chain
		/// </summary>
		/// <param name="value">Byte to add</param>
		public void AppendByte(byte value) {
			if (BitPointer == 0) {
				Bytes.Add(value);
			}
			else {
				byte firstPart = (byte)(value << BitPointer);
				byte secondPart = (byte)(value >> (8 - BitPointer));

				Bytes[Bytes.Count - 1] += firstPart;
				Bytes.Add(secondPart);
			}
		}
		/// <summary>
		/// Adds 8 bits to the chain at current pointer position
		/// </summary>
		/// <param name="bytes"></param>
		public void AppendBytes(IEnumerable<byte> bytes) {
			foreach(byte b in bytes) {
				AppendByte(b);
			}
		}

		public void Print() {
			foreach(var b in Bytes) {
				byte pointer = 0;
				for(int i = 0; i < 8; i++) {
					if (pointer == 0) pointer = 1;
					else pointer = (byte)(pointer << 1);

					Console.Write((b & pointer) == 0 ? '0' : '1');
				}
			}
			Console.WriteLine();
		}

		public List<byte> ToBytes() {
			return this.Bytes;
		}

		public ByteArrayReader ToByteArrayReadable() {
			return new ByteArrayReader(this.Bytes);
		}
	}
}
