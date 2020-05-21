using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BinaryConverter.models
{
	/// <summary>
	/// Class type for writing bytes stream to file. 
	/// NOTICE: The writer keeps the file stream open for as long as 
	/// </summary>
	public class BinaryFileWriter : IBinaryWritable, IDisposable
	{
		private BinaryWriter Writer;
		public bool Disposed { private set; get; }

		public BinaryFileWriter(string filePath, FileMode mode = FileMode.Create) {
			Writer = new BinaryWriter(new FileStream(filePath, mode));
			Disposed = false;
		}

		~BinaryFileWriter() {
			Dispose();
		}

		public void WriteByte(byte b) {
			Writer.Write(b);
		}

		public void WriteBytes(IEnumerable<byte> bytes) {
			foreach(byte b in bytes) {
				WriteByte(b);
			}
		}

		public void Dispose() {
			if (!Disposed) {
				Writer.Close();
				Writer.Dispose();

				Disposed = true;
			}
		}
	}
}
