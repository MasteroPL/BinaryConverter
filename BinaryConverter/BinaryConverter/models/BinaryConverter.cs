using BinaryConverter.attributes;
using BinaryConverter.exceptions;
using BinaryConverter.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BinaryConverter.models
{
	/// <summary>
	/// Class type used for converting objects into their binary representations and the other way around - recovers original objects (to a defined degree) from it's binary representation
	/// 
	/// Converters (Encoders and Decoders) for given type are used based on following prioritization (top to bottom):
	/// -> 1. A custom converter provided as argument for conversion (Out of scope for this class particularly [as it doesn't make any sense here], used in util class "BinaryBuilder")
	/// -> 2. A custom converter added manually to the converters list within a class
	/// -> 3. A custom converter defined within the converter class
	/// -> 4. A custom converter defined within a converted type
	/// -> 5. A custom converter defined globally for given class type
	/// -> 6. A default converter added manually to the converters list within a class
	/// -> 7. A default converter defined within the converter class
	/// -> 8. A default converter defined for a serializable type
	/// -> 9. A default converter (defined within the converter class)
	/// </summary>
	public class BinaryConverter {
		private BinaryFormatter _binaryFormatter = new BinaryFormatter();

		private List<BinaryEncoderMethod> CustomEncoders = new List<BinaryEncoderMethod>();
		private List<BinaryDecoderMethod> CustomDecoders = new List<BinaryDecoderMethod>();
		private List<BinaryEncoderMethod> DefaultEncoders = new List<BinaryEncoderMethod>();
		private List<BinaryDecoderMethod> DefaultDecoders = new List<BinaryDecoderMethod>();

		private BinaryEncoderMethod[] _predefinedCustomEncoders;
		private BinaryDecoderMethod[] _predefinedCustomDecoders;
		private BinaryEncoderMethod[] _predefinedDefaultEncoders;
		private BinaryDecoderMethod[] _predefinedDefaultDecoders;

		public Encoding StringEncoding { get; set; }

		public BinaryConverter(Encoding stringEncoding = null) {
			StringEncoding = (stringEncoding == null) ? Encoding.UTF8 : stringEncoding; // default encoding

			// Temporary
			_predefinedCustomEncoders = new BinaryEncoderMethod[] { };
			_predefinedCustomDecoders = new BinaryDecoderMethod[] { };
			_predefinedDefaultEncoders = new BinaryEncoderMethod[] { };
			_predefinedDefaultDecoders = new BinaryDecoderMethod[] { };
		}

		/// <summary>
		/// Encoder specifically dedicated to encoding Type
		/// </summary>
		/// <param name="type">Type to convert</param>
		/// <returns>Bytes representation of of the type</returns>
		protected byte[] EncodeType(Type type) {

			//string encoded = type.FullName + "," + type.Assembly.GetName().Cu
			throw new NotImplementedException();

		}

		/// <summary>
		/// Decoder specifically dedicated to decoding Type
		/// </summary>
		/// <param name="bytes">Bytes to convert from</param>
		/// <returns>Type represented by the bytes</returns>
		protected Type DecodeType(byte[] bytes) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Searches through provided list in order to find a converter defined for given type
		/// </summary>
		/// <typeparam name="T">Converter type (Encoder|Decoder)</typeparam>
		/// <param name="convertersList">List to search through</param>
		/// <param name="type">Type to search for</param>
		/// <returns>Converter for the type</returns>
		protected T SearchConvertersList<T>(IEnumerable<T> convertersList, Type type) where T : BinaryConverterMethod {
			foreach (T converter in convertersList) {
				if(converter.ForType == type) {
					return converter;
				}
			}
			return null;
		}

		#region GetEcnoder and assiociated methods
		/// <summary>
		/// Attempts to find a custom encoder for given type within defined ones
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryEncoderMethod instance found or null when unable to find matching object</returns>
		public BinaryEncoderMethod GetCustomEncoder(Type type) {
			return SearchConvertersList<BinaryEncoderMethod>(CustomEncoders, type);
		}
		/// <summary>
		/// Attempts to find a predefined custom encoder for given type within predefined ones
		/// 
		/// Predefined encoders are assigned upon this class initialization based on methods marked with BinaryConverterCustomEncoderMethodAttribute
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryEncoderMethod instance found or null when unable to find matching object</returns>
		public BinaryEncoderMethod GetPredefinedCustomEncoder(Type type) {
			return SearchConvertersList<BinaryEncoderMethod>(_predefinedCustomEncoders, type);
		}

		/// <summary>
		/// Searches for a method defined in provided object as it's custom converter
		/// </summary>
		/// <param name="o">Object to search within</param>
		/// <returns>Method defined as provided object's custom converter or null if not found</returns>
		public Func<object, IEnumerable<byte>> GetCustomClassEncoder(object o) {
			Type type = o.GetType();
			var methods = type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			
			foreach(var m in methods) {
				if(m.GetCustomAttributes(typeof(CustomEncoderMethodAttribute), false).Length > 0) {
					if (!m.IsStatic) {
						return (input) => {
							try {
								return (IEnumerable<byte>)m.Invoke(o, new object[] { input });
							} catch (Exception e) {
								throw new ConverterMethodException(o, m,
									"An error occured while attempting to use custom class encoder. " +
									"\nObject: " + o.ToString() +
									"\nMethod: " + m.ToString() +
									"\nVerify whether this method is properly defined and does not produce exceptions!" +
									"\nInner exception: " + e.ToString()
								, e);
							}
						};
					}
					else {
						return (input) => {
							try {
								return (IEnumerable<byte>)m.Invoke(null, new object[] { input });
							} catch (Exception e) {
								throw new ConverterMethodException(o, m,
									"An error occured while attempting to use custom class encoder. " +
									"\nObject: " + o.ToString() +
									"\nMethod: " + m.ToString() +
									"\nVerify whether this method is properly defined and does not produce exceptions!" +
									"\nInner exception: " + e.ToString()
								, e);
							}
						};
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Searches for a globally defined encoder method for given type
		/// </summary>
		/// <param name="type">Type search through</param>
		/// <returns>Encoder method or null if not found</returns>
		public Func<object, IEnumerable<byte>> GetGlobalClassEncoder(Type type) {
			var attribute = type.GetCustomAttributes(
				typeof(ClassEncoderAttribute),
				false // Not considering inheritance (attribute cannot be inherited)
			).FirstOrDefault() as ClassEncoderAttribute;

			if(attribute != default(ClassEncoderAttribute)) {
				return attribute.Encoder;
			}
			return null;
		}

		/// <summary>
		/// Attempts to find a default encoder for given type within defined ones
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryEncoderMethod instance found or null when unable to find matching object</returns>
		public BinaryEncoderMethod GetDefaultEncoder(Type type) {
			return SearchConvertersList<BinaryEncoderMethod>(DefaultEncoders, type);
		}

		/// <summary>
		/// Attempts to find a default encoder for given type within predefined ones
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryEncoderMethod instance found or null when unable to find matching object</returns>
		public BinaryEncoderMethod GetPredefinedDefaultEncoder(Type type) {
			return SearchConvertersList<BinaryEncoderMethod>(_predefinedDefaultEncoders, type);
		}

		/// <summary>
		/// Finds the appropriate encoder for provided object
		/// </summary>
		/// <param name="o">Object to find the encoder for</param>
		/// <returns>An encoder method appropriate for that object</returns>
		protected virtual BinaryEncoderData GetEncoder(object o) {
			Type objectType = o.GetType();
			BinaryEncoderMethod encoder;
			Func<object, IEnumerable<byte>> encoderMethod;

			// Step 1. Looking for a custom converter added manually to the converters list
			encoder = GetCustomEncoder(objectType);
			if(encoder != null) {
				// Found the appropriate encoder
				return new BinaryEncoderData(encoder.Encoder);
			}

			//  Step 2. Looking for a custom converter defined within the converter class
			encoder = GetPredefinedCustomEncoder(objectType);
			if(encoder != null) {
				// Found the appropriate encoder
				return new BinaryEncoderData(encoder.Encoder);
			}

			// Step 3. Looking for a custom converter defined within the converted class
			encoderMethod = GetCustomClassEncoder(o);
			if(encoderMethod != null) {
				return new BinaryEncoderData(encoderMethod);
			}

			// Step 4. Looking for a global converter defined for the class
			encoderMethod = GetGlobalClassEncoder(objectType);
			if(encoderMethod != null) {
				return new BinaryEncoderData(encoderMethod);
			}

			// Step 5. Looking for a default converter added manually to the converters list
			encoder = GetDefaultEncoder(objectType);
			if(encoder != null) {
				return new BinaryEncoderData(encoder.Encoder);
			}

			// Step 6. Looking for a default converter defined within the converter class
			encoder = GetPredefinedDefaultEncoder(objectType);
			if(encoder != null) {
				return new BinaryEncoderData(encoder.Encoder);
			}

			// Step 7. Attempting a converter for a primitive type
			if (objectType.IsPrimitive) {
				return new BinaryEncoderData(PrimitiveTypeEncoder, ConverterType.INCLUSIVE_PRIMITIVE);
			}

			// Step 8. Attempting a converter for a serializable type
			if (objectType.IsSerializable) {
				return new BinaryEncoderData(SerializableTypeEncoder, ConverterType.INCLUSIVE_SERIALIZABLE);
			}

			// Step 9. None of the above is valid, apply the default encoder
			return new BinaryEncoderData(ComplexTypeEncoder);
		}

		/// <summary>
		/// Finds the appriopriate encoder within specified category
		/// </summary>
		/// <param name="o">Object to find the encoder for</param>
		/// <param name="category">Category of encoders to consider</param>
		/// <returns>An encoder method appropriate for that object or null if not found</returns>
		protected virtual BinaryEncoderData GetSpecificEncoder(object o, ConverterCategory category) {
			BinaryEncoderMethod encoder;
			Func<object, IEnumerable<byte>> encoderMethod;
			switch (category) {
				case ConverterCategory.ANY:
					return GetEncoder(o);

				case ConverterCategory.CUSTOM:
					encoder = GetCustomEncoder(o.GetType());
					return (encoder == null) ? null : new BinaryEncoderData(encoder.Encoder);

				case ConverterCategory.CUSTOM_PREDEFINED:
					encoder = GetPredefinedCustomEncoder(o.GetType());
					return (encoder == null) ? null : new BinaryEncoderData(encoder.Encoder);

				case ConverterCategory.CUSTOM_FOR_CLASS:
					encoderMethod = GetCustomClassEncoder(o);
					return (encoderMethod == null) ? null : new BinaryEncoderData(encoderMethod);

				case ConverterCategory.GLOBAL_FOR_CLASS:
					encoderMethod = GetGlobalClassEncoder(o.GetType());
					return (encoderMethod == null) ? null : new BinaryEncoderData(encoderMethod);

				case ConverterCategory.DEFAULT:
					encoder = GetDefaultEncoder(o.GetType());
					return (encoder == null) ? null : new BinaryEncoderData(encoder.Encoder);

				case ConverterCategory.DEFAULT_PREDEFINED:
					encoder = GetPredefinedDefaultEncoder(o.GetType());
					return (encoder == null) ? null : new BinaryEncoderData(encoder.Encoder);

				case ConverterCategory.SERIALIZABLE:
					if (o.GetType().IsSerializable) {
						return new BinaryEncoderData(SerializableTypeEncoder);
					}
					return null;

				case ConverterCategory.COMPLEX:
					if (!o.GetType().IsPrimitive) {
						return new BinaryEncoderData(ComplexTypeEncoder);
					}
					return null;

				default:
					throw new NotImplementedException("Provided category was not implemented");
			}
		}
		#endregion

		public virtual BinaryCodeBuilder Encode(object o, ConverterCategory converterCategory = ConverterCategory.ANY, BinaryCodeBuilder builder = null) {
			if(builder == null) {
				builder = new BinaryCodeBuilder();
			}

			BinaryEncoderData encoderData = GetSpecificEncoder(o, converterCategory);
			IEnumerable<byte> encodedObject = encoderData.Encoder.Invoke(o);

			if (encoderData.ConverterType == ConverterType.EXCLUSIVE) {
				builder.AppendBit(0);
				// TODO: encode type
			}
			else {
				builder.AppendBit(1);

				if(encoderData.ConverterType == ConverterType.INCLUSIVE_SERIALIZABLE) {
					builder.AppendBit(0);
				}
				else {
					// Inclusive primitive
					builder.AppendBit(1);
				}
			}


			uint length = (uint)encodedObject.Count();
			byte lengthSizeInBytes;
			if(length < 256) { lengthSizeInBytes = 0; }
			else if(length < 65536) { lengthSizeInBytes = 1; }
			else if(length < 16777216) { lengthSizeInBytes = 2; }
			else { lengthSizeInBytes = 3; }

			builder.AppendBit((byte)(lengthSizeInBytes % 2));
			builder.AppendBit((byte)((lengthSizeInBytes / 2) % 2));

			byte[] lengthBytes = BitConverter.GetBytes(length);
			for(int i = 0; i <= lengthSizeInBytes; i++) {
				builder.AppendByte(lengthBytes[i]);
			}

			builder.AppendBytes(encodedObject);

			return builder;
		}

		protected virtual IEnumerable<byte> PrimitiveTypeEncoder(object o) {
			Type t = o.GetType();
			byte[] bytes;
			TypeCode code;

			if (t == typeof(bool)) {
				bytes = BitConverter.GetBytes((bool)o);
				code = TypeCode.Boolean;
			}
			else if (t == typeof(char)) {
				bytes = BitConverter.GetBytes((char)o);
				code = TypeCode.Char;
			}
			else if (t == typeof(short)) {
				bytes = BitConverter.GetBytes((short)o);
				code = TypeCode.Int16;
			}
			else if (t == typeof(ushort)) {
				bytes = BitConverter.GetBytes((ushort)o);
				code = TypeCode.UInt16;
			}
			else if (t == typeof(int)) {
				bytes = BitConverter.GetBytes((int)o);
				code = TypeCode.Int32;
			}
			else if (t == typeof(uint)) {
				bytes = BitConverter.GetBytes((uint)o);
				code = TypeCode.UInt32;
			}
			else if (t == typeof(long)) {
				bytes = BitConverter.GetBytes((long)o);
				code = TypeCode.Int64;
			}
			else if (t == typeof(ulong)) {
				bytes = BitConverter.GetBytes((ulong)o);
				code = TypeCode.UInt64;
			}
			else if (t == typeof(float)) {
				bytes = BitConverter.GetBytes((float)o);
				code = TypeCode.Single;
			}
			else if (t == typeof(double)) {
				bytes = BitConverter.GetBytes((double)o);
				code = TypeCode.Double;
			}
			else if(t == typeof(byte)) {
				bytes = new byte[] { (byte)o };
				code = TypeCode.Byte;
			}
			else if(t == typeof(SByte)) {
				byte b = unchecked((byte)o); // Conversion with ignoring of overflow
				bytes = new byte[] { b };
				code = TypeCode.SByte;
			}
			else
				throw new ArgumentException("Object provided is not a primitive type one or unable to process it (perhaps a default converter is missing?)");

			var result = new byte[bytes.Length + 1];
			result[0] = (byte)code; // First byte defines the type

			for(int i = 0; i < bytes.Length; i++) {
				result[i + 1] = bytes[i];
			}

			return result;
		}

		protected virtual IEnumerable<byte> SerializableTypeEncoder(object o) {
			using (MemoryStream ms = new MemoryStream()) {
				_binaryFormatter.Serialize(ms, o);

				return ms.ToArray();
			}
		}

		protected virtual IEnumerable<byte> ComplexTypeEncoder(object o) {
			throw new NotImplementedException();
		}

		public virtual object PrimitiveTypeDecoder(IEnumerable<byte> bytes) {
			byte[] objectBytes = new byte[bytes.Count() - 1];
			bool first = true;
			byte type = 0;
			int index = 0;
			object o;

			// Splitting received array into 2 parts: type part, and object part
			foreach(var b in bytes) {
				if (first) {
					type = b;
					first = false;
				}
				else {
					objectBytes[index] = b;
					index++;
				}
			}

			if(type == (byte)TypeCode.Boolean) {
				o = Convert.ToBoolean(objectBytes[0]);
			}
			else if(type == (byte)TypeCode.Char) {
				o = Convert.ToChar(objectBytes[0]);
			}
			else if(type == (byte)TypeCode.Int16) {
				o = BitConverter.ToInt16(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.UInt16) {
				o = BitConverter.ToUInt16(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.Int32) {
				o = BitConverter.ToInt32(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.UInt32) {
				o = BitConverter.ToUInt32(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.Int64) {
				o = BitConverter.ToInt64(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.UInt64) {
				o = BitConverter.ToUInt64(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.Single) {
				o = BitConverter.ToSingle(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.Double) {
				o = BitConverter.ToDouble(objectBytes, 0);
			}
			else if (type == (byte)TypeCode.Byte) {
				o = objectBytes[0];
			}
			else if (type == (byte)TypeCode.SByte) {
				o = unchecked((SByte)objectBytes[0]);
			}
			else {
				// TODO: change to a proper exception
				throw new ArgumentException();
			}

			return o;
		}

		protected virtual object SerializableTypeDecoder(IEnumerable<byte> bytes) {
			using (MemoryStream ms = new MemoryStream()) {
				ms.Write(bytes.ToArray(), 0, bytes.Count());
				ms.Seek(0, SeekOrigin.Begin);
				return _binaryFormatter.Deserialize(ms);
			}
		}

		#region Predefined Encoders
		/*
		protected virtual IEnumerable<byte> StringTypeEncoder(object o) {
			return StringEncoding.GetBytes((string)o);
		}
		*/
		#endregion

		#region Predefined Decoders
		/*
	protected virtual object StringTypeDecoder(IEnumerable<byte> bytes) {
		return StringEncoding.GetString(bytes.ToArray());
	}
	*/
		#endregion
	}
}
