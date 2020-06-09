using System;
using System.Collections.Generic;
using System.Text;

using BinaryConverter.models;

namespace BinaryConverter.utils
{
	public class BinaryCodeReader
	{
		private IBinaryReadable Source;
		public byte BitPointer { private set; get; }
		private byte CurrentByte;
		public int BytePointer { private set; get; }
		private List<byte> BytesHistory = new List<byte>();

		public BinaryCodeReader(IBinaryReadable source) {
			if (source == null) {
				throw new ArgumentNullException("IBinaryReadable cannot be a null instance");
			}

			Source = source;
			BytePointer = 0;
			BitPointer = 0;
		}
		public BinaryCodeReader(IEnumerable<byte> source) {
			if (source == null) {
				throw new ArgumentNullException("Source cannot be a null instance");
			}

			Source = new ByteArrayReader(source);
			BytePointer = 0;
			BitPointer = 0;
		}

		protected byte GetNextByte() {
			if (BytePointer == BytesHistory.Count) {
				if (!Source.EndOfSource()) {
					CurrentByte = Source.GetNextByte();
					BytesHistory.Add(CurrentByte);
					BytePointer++;
					return CurrentByte;
				}
				else {
					throw new IndexOutOfRangeException("Attempted to read beyond source range");
				}
			}
			else {
				BytePointer++;
				CurrentByte = BytesHistory[BytePointer - 1];
				return CurrentByte;
			}			
		}

		/// <summary>
		/// Reads next single bit from the source
		/// </summary>
		/// <returns>A value of that single bit (either 1 or 0)</returns>
		public byte ReadNextBit() {
			byte result;
			int conjuctor = 1; // for the sake of not converting to byte it's as int

			// Special case, requires reading the next byte from source
			if (BitPointer == 0) {
				CurrentByte = GetNextByte();
			}

			conjuctor = (conjuctor << BitPointer);
			result = (byte)((CurrentByte & conjuctor) >> BitPointer);

			BitPointer = (BitPointer == 7) ? (byte)0 : (byte)(BitPointer + 1);
			

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
				GetNextByte();
				return CurrentByte;
			}

			byte result = (byte)(CurrentByte >> BitPointer);
			GetNextByte();
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
			if (BytePointer == BytesHistory.Count) {
				return Source.EndOfSource();
			}
			else {
				return false;
			}
		}

		public bool EndOfBits() {
			if(EndOfSource() && BitPointer == 0) {
				return true;
			}
			return false;
		}

		public void MoveBitPointer(int offset) {
			int bytesToMove = offset / 8;
			int bitsToMove = offset - bytesToMove * 8;

			if (BytePointer + bytesToMove <= BytesHistory.Count) {
				if (BytePointer + bytesToMove < 0) {
					throw new IndexOutOfRangeException("Cannot go back in bytes history beyond the beginning of the source");
				}
				BytePointer += bytesToMove;
			}
			else {
				int remainingBytesOffset = BytePointer + bytesToMove - BytesHistory.Count;
				BytePointer = BytesHistory.Count;

				for(int i = 0; i < remainingBytesOffset; i++) {
					GetNextByte();
				}
			}

			short pointer = (short)(BitPointer + bitsToMove);
			if(pointer > 7) {
				GetNextByte();
				pointer = (byte)(pointer - 8);
			}
			if(pointer < 0) {
				BytePointer -= 1;
				if (BytePointer < 0) {
					throw new IndexOutOfRangeException("Cannot go back in bytes history beyond the beginning of the source");
				}
				pointer = (byte)(pointer + 8);
			}
			BitPointer = (byte)pointer;
		}

		public void MoveBytePointer(int offset) {
			MoveBitPointer(offset * 8);
		}
	}
}
