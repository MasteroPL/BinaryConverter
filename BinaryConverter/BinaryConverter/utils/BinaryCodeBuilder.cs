using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.utils
{
	/// <summary>
	/// Similarly to StringBuilder, you can use it to create a binary chain bit by bit, byte by byte or mixing the two
	/// </summary>
	public class BinaryCodeBuilder
	{
		/// <summary>
		/// Points at the current byte position
		/// </summary>
		private int CurrentWriteBytePointer = 0;
		/// <summary>
		/// Points at the current bit position in the current byte (takes values only from range 0-7)
		/// </summary>
		private byte CurrentWriteBitPointer = 0;
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
		/// Adds a single bit to the chain at current pointer location
		/// </summary>
		/// <param name="bit">Single bit value</param>
		public void AppendBit(byte bit) {
			if(bit != 0 && bit != 1) {
				throw new ArgumentOutOfRangeException("bit argument can only take values of 0 and 1");
			}

			// Filling in missing bytes
			while(Bytes.Count <= CurrentWriteBytePointer) {
				Bytes.Add(0);
			}

			byte tmp = bit;
			tmp = (byte)(tmp << CurrentWriteBitPointer);

			byte highPart = (byte)(1 << CurrentWriteBitPointer);
			byte lowPart = (byte)(~((~0) << (CurrentWriteBitPointer)));

			Bytes[CurrentWriteBytePointer] = (byte)((Bytes[CurrentWriteBytePointer] & lowPart) + (tmp & highPart));

			CurrentWriteBitPointer++;
			if(CurrentWriteBitPointer == 8) {
				CurrentWriteBitPointer = 0;
				CurrentWriteBytePointer++;
			}
		}
		/// <summary>
		/// Adds a series of bits to the chain at the current pointer position
		/// </summary>
		/// <param name="bits">List of bits to add</param>
		public void AppendBits(IEnumerable<byte> bits) {
			int count = 0;
			foreach (var b in bits) {
				if (b != 0 && b != 1) {
					throw new ArgumentOutOfRangeException("bit argument can only take values of 0 and 1");
				}
				count++;
			}

			var spaceLeftAfterAdding = Bytes.Count * 8 - (CurrentWriteBytePointer) * 8 - CurrentWriteBitPointer - count;
			while(spaceLeftAfterAdding < 0) {
				Bytes.Add(0);
				spaceLeftAfterAdding += 8;
			}

			var tmpBitPointer = CurrentWriteBitPointer;
			var tmpBytePointer = CurrentWriteBytePointer;
			bool firstByte = true;
			int current = 0;

			// For one time use only
			byte lowPart = (byte)(~((~0) << (CurrentWriteBitPointer)));

			foreach (var b in bits) {
				current += (b << tmpBitPointer);

				tmpBitPointer++;
				if(tmpBitPointer == 8) {
					if (firstByte) {
						Bytes[tmpBytePointer] = (byte)((Bytes[tmpBytePointer] & lowPart) + current);
						firstByte = false;
						current = 0;
					}
					else {
						Bytes[tmpBytePointer] = (byte)current;
						current = 0;
					}

					tmpBitPointer = 0;
					tmpBytePointer++;
				}
			}

			if(tmpBitPointer != 0) {
				Bytes[tmpBytePointer] = (byte)current;
			}

			CurrentWriteBitPointer = tmpBitPointer;
			CurrentWriteBytePointer = tmpBytePointer;
		}
		/// <summary>
		/// Adds 8 bits to the chain at current pointer position
		/// </summary>
		/// <param name="value">Byte to add</param>
		public void AppendByte(byte value) {
			while(Bytes.Count <= CurrentWriteBytePointer) {
				Bytes.Add(0);
			}

			if (CurrentWriteBitPointer == 0) {
				Bytes[CurrentWriteBytePointer] = value;
			}
			else {
				if(Bytes.Count <= CurrentWriteBytePointer + 1) {
					Bytes.Add(0);
				}

				byte lowPart = (byte)(~((~0) << (CurrentWriteBitPointer)));
				byte firstByteValue = (byte)((value << CurrentWriteBitPointer) & 255);
				byte secondByteValue = (byte)(value >> (8 - CurrentWriteBitPointer));

				Bytes[CurrentWriteBytePointer] = (byte)((Bytes[CurrentWriteBytePointer] & lowPart) + firstByteValue);
				Bytes[CurrentWriteBytePointer + 1] = secondByteValue;
			}

			CurrentWriteBytePointer++;
		}
		/// <summary>
		/// Adds 8 bits to the chain at current pointer position
		/// </summary>
		/// <param name="bytes"></param>
		public void AppendBytes(IEnumerable<byte> bytes) {
			while (Bytes.Count <= CurrentWriteBytePointer) {
				Bytes.Add(0);
			}

			int tmpBytePointer = CurrentWriteBytePointer;
			byte tmpBitPointer = CurrentWriteBitPointer;

			// Simple case -> just add the bytes at proper indices
			if (CurrentWriteBitPointer == 0) {
				foreach (var b in bytes) {
					if (tmpBytePointer == Bytes.Count) {
						Bytes.Add(b);
					}
					else {
						Bytes[tmpBytePointer] = b;
					}
					tmpBytePointer++;
				}
			}
			// Complex case -> Every byte will be split into 2 indices
			else {
				foreach(var b in bytes) {
					AppendByte(b);
				}
			}

			CurrentWriteBitPointer = tmpBitPointer;
			CurrentWriteBytePointer = tmpBytePointer;
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
	}
}
