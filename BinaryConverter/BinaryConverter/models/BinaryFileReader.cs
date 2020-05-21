using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinaryConverter.models
{
	/// <summary>
	/// Class type for reading bytes straight from a filestream.
	/// NOTICE: This class keeps the file stream open until it's destructor is called, or until manual call its method "Dispose"
	/// </summary>
	public class BinaryFileReader : IBinaryReadable, IDisposable
	{
		private FileStream BinaryFileStream;
		public bool Disposed { private set; get; }

		public BinaryFileReader(string filePath) {
			BinaryFileStream = new FileStream(filePath, FileMode.Open);
			Disposed = false;
		}

		~BinaryFileReader() {
			Dispose();
		}

		public void Dispose() {
			if (!Disposed) {
				BinaryFileStream.Close();
				BinaryFileStream.Dispose();
				Disposed = true;
			}
		}

		public bool EndOfSource() {
			return BinaryFileStream.Position == BinaryFileStream.Length;
		}

		public byte GetNextByte() {
			return (byte)BinaryFileStream.ReadByte();
		}
	}
}
