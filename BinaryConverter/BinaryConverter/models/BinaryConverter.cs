using BinaryConverter.attributes;
using BinaryConverter.exceptions;
using BinaryConverter.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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
	/// -> 8. A default converter defined for a primitive type
	/// -> 9. A default converter defined for a serializable type
	/// -> 10. A default converter (defined within the converter class)
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

		protected Type CurrentDecodedType = null;

		public Encoding StringEncoding { get; set; }
		public Encoding CodeEncoding { get; set; }

		public BinaryConverter(Encoding stringEncoding = null, Encoding codeEncoding = null) {
			StringEncoding = (stringEncoding == null) ? Encoding.UTF8 : stringEncoding; // default encoding
			CodeEncoding = (codeEncoding == null) ? Encoding.UTF8 : codeEncoding; // default encoding

			// Temporary
			_predefinedCustomEncoders = new BinaryEncoderMethod[] { };
			_predefinedCustomDecoders = new BinaryDecoderMethod[] { };
			_predefinedDefaultEncoders = new BinaryEncoderMethod[] { };
			_predefinedDefaultDecoders = new BinaryDecoderMethod[] { };
		}

		protected byte GetNumberOfBytesToStoreLength(uint length) {
			if (length < 256) { return 1; } // length < 2^8
			else if (length < 65536) { return 2; } // length < 2^16
			else if (length < 16777216) { return 3; } // length < 2^24
			else { return 4; }
		}

		protected BinaryCodeBuilder EncodeLength(uint length) {
			byte lengthSizeInBytes = GetNumberOfBytesToStoreLength(length);

			BinaryCodeBuilder builder = new BinaryCodeBuilder();

			builder.AppendBit((byte)(lengthSizeInBytes % 2)); 
			builder.AppendBit((byte)((lengthSizeInBytes / 2) % 2)); // Minimum length is 1 byte so we start counting that value with 0, meaning 0 represents 1, 1 represents 2 etc. 

			byte[] lengthBytes = BitConverter.GetBytes(length);
			for (int i = 0; i <= lengthSizeInBytes; i++) {
				builder.AppendByte(lengthBytes[i]);
			}

			return builder;
		}

		protected uint DecodeLength(BinaryCodeReader reader) {
			int lengthSizeInBytes = (reader.ReadNextBit() + 1) + reader.ReadNextBit() * 2;
			var lengthBytes = new byte[4] { 0, 0, 0, 0 };
			for (int i = 0; i < lengthSizeInBytes; i++) {
				lengthBytes[i] = reader.ReadNextByte();
			}
			return BitConverter.ToUInt32(lengthBytes, 0);
		}

		#region Converter Functionalities Encoders
		/// <summary>
		/// Encoder specifically dedicated to encoding Type
		/// </summary>
		/// <param name="type">Type to convert</param>
		/// <returns>Bytes representation of of the type</returns>
		public byte[] EncodeType(Type type) {
			return CodeEncoding.GetBytes(type.AssemblyQualifiedName);
		}

		public byte[] EncodeFieldName(string fieldName) {
			return CodeEncoding.GetBytes(fieldName);
		}
		#endregion

		#region Converter Functionalities Decoders
		/// <summary>
		/// Decoder specifically dedicated to decoding Type
		/// </summary>
		/// <param name="bytes">Bytes to convert from</param>
		/// <returns>Type represented by the bytes</returns>
		public Type DecodeType(byte[] bytes) {
			return Type.GetType(CodeEncoding.GetString(bytes));
		}

		public string DecodeFieldName(byte[] bytes) {
			return CodeEncoding.GetString(bytes);
		}
		#endregion

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

		#region GetEncoder and assiociated methods
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
			if(o == null) {
				return new BinaryEncoderData(null, ConverterType.NULL);
			}

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

				case ConverterCategory.PRIMITIVE:
					if (o.GetType().IsSerializable) {
						return new BinaryEncoderData(PrimitiveTypeEncoder);
					}
					return null;

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
			IEnumerable<byte> encodedObject = null;

			if(encoderData.ConverterType == ConverterType.NULL) {
				builder.AppendBit(0);
				builder.AppendBit(0);

				return builder;
			}
			else if (encoderData.ConverterType == ConverterType.EXCLUSIVE) {
				encodedObject = encoderData.Encoder.Invoke(o);
				builder.AppendBit(0);
				builder.AppendBit(1);
				byte[] typeBytes = EncodeType(o.GetType());
				builder.Append(EncodeLength((uint)typeBytes.Length));
				builder.AppendBytes(typeBytes);
			}
			else {
				encodedObject = encoderData.Encoder.Invoke(o);
				builder.AppendBit(1);

				if(encoderData.ConverterType == ConverterType.INCLUSIVE_SERIALIZABLE) {
					// Inclusive serializable, no need to encode type
					builder.AppendBit(0);
				}
				else {
					// Inclusive primitive, no need to encode type
					builder.AppendBit(1);
				}
			}

			uint length = (uint)encodedObject.Count();
			builder.Append(EncodeLength(length));

			builder.AppendBytes(encodedObject);

			return builder;
		}

		#region GetDecoder and assiociated methods
		/// <summary>
		/// Attempts to find a custom decoder for given type within defined ones
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryDecoderMethod instance found or null when unable to find matching object</returns>
		public BinaryDecoderMethod GetCustomDecoder(Type type) {
			return SearchConvertersList<BinaryDecoderMethod>(CustomDecoders, type);
		}
		/// <summary>
		/// Attempts to find a predefined custom decoder for given type within predefined ones
		/// 
		/// Predefined encoders are assigned upon this class initialization based on methods marked with BinaryConverterCustomEncoderMethodAttribute
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryEncoderMethod instance found or null when unable to find matching object</returns>
		public BinaryDecoderMethod GetPredefinedCustomDecoder(Type type) {
			return SearchConvertersList<BinaryDecoderMethod>(_predefinedCustomDecoders, type);
		}

		/// <summary>
		/// Searches for a method defined in provided object as it's custom converter
		/// </summary>
		/// <param name="o">Object to search within</param>
		/// <returns>Method defined as provided object's custom converter or null if not found</returns>
		public Func<object, IEnumerable<byte>> GetCustomClassDecoder(object o) {
			Type type = o.GetType();
			var methods = type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			foreach (var m in methods) {
				if (m.GetCustomAttributes(typeof(CustomDecoderMethodAttribute), false).Length > 0) {
					if (!m.IsStatic) {
						return (input) => {
							try {
								return (IEnumerable<byte>)m.Invoke(o, new object[] { input });
							} catch (Exception e) {
								throw new ConverterMethodException(o, m,
									"An error occured while attempting to use custom class decoder. " +
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
									"An error occured while attempting to use custom class decoder. " +
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
		/// Searches for a globally defined decoder method for given type
		/// </summary>
		/// <param name="type">Type to search through</param>
		/// <returns>Decoder method or null if not found</returns>
		public Func<IEnumerable<byte>, object> GetGlobalClassDecoder(Type type) {
			var attribute = type.GetCustomAttributes(
				typeof(ClassDecoderAttribute),
				false // Not considering inheritance (attribute cannot be inherited)
			).FirstOrDefault() as ClassDecoderAttribute;

			if (attribute != default(ClassDecoderAttribute)) {
				return attribute.Decoder;
			}
			return null;
		}

		/// <summary>
		/// Attempts to find a default decoder for given type within defined ones
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryDecoderMethod instance found or null when unable to find matching object</returns>
		public BinaryDecoderMethod GetDefaultDecoder(Type type) {
			return SearchConvertersList<BinaryDecoderMethod>(DefaultDecoders, type);
		}

		/// <summary>
		/// Attempts to find a default decoder for given type within predefined ones
		/// </summary>
		/// <param name="type">Type to look for</param>
		/// <returns>BinaryDecoderMethod instance found or null when unable to find matching object</returns>
		public BinaryDecoderMethod GetPredefinedDefaultDecoder(Type type) {
			return SearchConvertersList<BinaryDecoderMethod>(_predefinedDefaultDecoders, type);
		}

		/// <summary>
		/// Finds the appropriate decoder for provided object
		/// </summary>
		/// <param name="o">Object to find the decoder for</param>
		/// <returns>An decoder method appropriate for that object</returns>
		protected virtual BinaryDecoderData GetDecoder(object o, ConverterCategory[] disallowed = null) {
			Type objectType = o.GetType();
			BinaryDecoderMethod decoder;
			Func<IEnumerable<byte>, object> decoderMethod;

			if (disallowed == null) disallowed = new ConverterCategory[] { };

			// Step 1. Looking for a custom converter added manually to the converters list
			decoder = GetCustomDecoder(objectType);
			if (decoder != null && !disallowed.Contains(ConverterCategory.CUSTOM)) {
				// Found the appropriate decoder
				return new BinaryDecoderData(decoder.Decoder);
			}

			//  Step 2. Looking for a custom converter defined within the converter class
			decoder = GetPredefinedCustomDecoder(objectType);
			if (decoder != null && !disallowed.Contains(ConverterCategory.CUSTOM_PREDEFINED)) {
				// Found the appropriate decoder
				return new BinaryDecoderData(decoder.Decoder);
			}

			// Step 3. Looking for a custom converter defined within the converted class
			decoderMethod = GetCustomClassDecoder(o);
			if (decoderMethod != null && !disallowed.Contains(ConverterCategory.CUSTOM_FOR_CLASS)) {
				return new BinaryDecoderData(decoderMethod);
			}

			// Step 4. Looking for a global converter defined for the class
			decoderMethod = GetGlobalClassDecoder(objectType);
			if (decoderMethod != null && !disallowed.Contains(ConverterCategory.GLOBAL_FOR_CLASS)) {
				return new BinaryDecoderData(decoderMethod);
			}

			// Step 5. Looking for a default converter added manually to the converters list
			decoder = GetDefaultDecoder(objectType);
			if (decoder != null && !disallowed.Contains(ConverterCategory.DEFAULT)) {
				return new BinaryDecoderData(decoder.Decoder);
			}

			// Step 6. Looking for a default converter defined within the converter class
			decoder = GetPredefinedDefaultDecoder(objectType);
			if (decoder != null && !disallowed.Contains(ConverterCategory.DEFAULT_PREDEFINED)) {
				return new BinaryDecoderData(decoder.Decoder);
			}

			// Step 7. Attempting a converter for a primitive type
			if (objectType.IsPrimitive && !disallowed.Contains(ConverterCategory.PRIMITIVE)) {
				return new BinaryDecoderData(PrimitiveTypeDecoder, ConverterType.INCLUSIVE_PRIMITIVE);
			}

			// Step 8. Attempting a converter for a serializable type
			if (objectType.IsSerializable && !disallowed.Contains(ConverterCategory.SERIALIZABLE)) {
				return new BinaryDecoderData(SerializableTypeDecoder, ConverterType.INCLUSIVE_SERIALIZABLE);
			}

			// Step 9. None of the above is valid, apply the default decoder
			if(!disallowed.Contains(ConverterCategory.COMPLEX))
				return new BinaryDecoderData(ComplexTypeDecoder);

			return null;
		}

		/// <summary>
		/// Finds the appriopriate decoder within specified category
		/// </summary>
		/// <param name="o">Object to find the decoder for</param>
		/// <param name="category">Category of decoders to consider</param>
		/// <returns>An encoder method appropriate for that object or null if not found</returns>
		protected virtual BinaryDecoderData GetSpecificDecoder(object o, ConverterCategory category, ConverterCategory[] disallowed = null) {
			BinaryDecoderMethod decoder;
			Func<IEnumerable<byte>, object> decoderMethod;

			if (disallowed == null) disallowed = new ConverterCategory[] { };

			switch (category) {
				case ConverterCategory.ANY:
					return GetDecoder(o);

				case ConverterCategory.CUSTOM:
					decoder = GetCustomDecoder(o.GetType());
					return (decoder == null && !disallowed.Contains(ConverterCategory.CUSTOM)) ? null : new BinaryDecoderData(decoder.Decoder);

				case ConverterCategory.CUSTOM_PREDEFINED:
					decoder = GetPredefinedCustomDecoder(o.GetType());
					return (decoder == null && !disallowed.Contains(ConverterCategory.CUSTOM_PREDEFINED)) ? null : new BinaryDecoderData(decoder.Decoder);

				case ConverterCategory.CUSTOM_FOR_CLASS:
					decoderMethod = GetCustomClassEncoder(o);
					return (decoderMethod == null && !disallowed.Contains(ConverterCategory.CUSTOM_FOR_CLASS)) ? null : new BinaryDecoderData(decoderMethod);

				case ConverterCategory.GLOBAL_FOR_CLASS:
					decoderMethod = GetGlobalClassDecoder(o.GetType());
					return (decoderMethod == null && !disallowed.Contains(ConverterCategory.GLOBAL_FOR_CLASS)) ? null : new BinaryDecoderData(decoderMethod);

				case ConverterCategory.DEFAULT:
					decoder = GetDefaultDecoder(o.GetType());
					return (decoder == null && !disallowed.Contains(ConverterCategory.DEFAULT)) ? null : new BinaryDecoderData(decoder.Decoder);

				case ConverterCategory.DEFAULT_PREDEFINED:
					decoder = GetPredefinedDefaultDecoder(o.GetType());
					return (decoder == null && !disallowed.Contains(ConverterCategory.DEFAULT_PREDEFINED)) ? null : new BinaryDecoderData(decoder.Decoder);

				case ConverterCategory.PRIMITIVE:
					if (o.GetType().IsPrimitive && !disallowed.Contains(ConverterCategory.PRIMITIVE)) {
						return new BinaryDecoderData(PrimitiveTypeDecoder);
					}
					return null;

				case ConverterCategory.SERIALIZABLE:
					if (o.GetType().IsSerializable && !disallowed.Contains(ConverterCategory.SERIALIZABLE)) {
						return new BinaryDecoderData(SerializableTypeDecoder);
					}
					return null;

				case ConverterCategory.COMPLEX:
					if (!o.GetType().IsPrimitive && !disallowed.Contains(ConverterCategory.COMPLEX)) {
						return new BinaryDecoderData(ComplexTypeDecoder);
					}
					return null;

				default:
					throw new NotImplementedException("Provided category was not implemented");
			}
		}
		#endregion

		public virtual object Decode(BinaryCodeReader bytes, ConverterCategory converterCategory = ConverterCategory.ANY) {
			byte tmpByte = bytes.ReadNextBit();
			CurrentDecodedType = null;

			if(tmpByte == 0) {
				tmpByte = bytes.ReadNextBit();
				if(tmpByte == 0) {
					// Null
					return null;
				}
				else {
					// Exclusive
					uint typeLength = DecodeLength(bytes);
					Type decodedType = DecodeType(bytes.ReadNextBytes(typeLength));
					CurrentDecodedType = decodedType;
					object uninitialized = FormatterServices.GetUninitializedObject(decodedType);

					var decoder = GetSpecificDecoder(
						uninitialized,
						converterCategory,
						new ConverterCategory[] {
							ConverterCategory.PRIMITIVE,
							ConverterCategory.SERIALIZABLE // The only inclusive types when it comes to BinaryConverter
						}
					);

					if (decoder != null) {
						uint length = DecodeLength(bytes);

						object result = decoder.Decoder.Invoke(bytes.ReadNextBytes(length));
						CurrentDecodedType = null;
						return result;
					}
					else {
						CurrentDecodedType = null;
						throw new ArgumentException("Could not find proper decoder for that set of bytes within provided range. Type encoded as exclusive, of type " + decodedType.FullName);
					}
				}
			}
			else {
				tmpByte = bytes.ReadNextBit();

				if(tmpByte == 0) {
					// Inclusive serializable
					if(converterCategory != ConverterCategory.ANY && converterCategory != ConverterCategory.SERIALIZABLE) {
						throw new ArgumentException("Object is encoded as inclusive serializable, requested converter category not allowing for that kind of conversion");
					}

					uint length = DecodeLength(bytes);
					return SerializableTypeDecoder(bytes.ReadNextBytes(length));
				}
				else {
					// Inclusive serializable
					if (converterCategory != ConverterCategory.ANY && converterCategory != ConverterCategory.PRIMITIVE) {
						throw new ArgumentException("Object is encoded as inclusive primitive, requested converter category not allowing for that kind of conversion");
					}

					uint length = DecodeLength(bytes);
					return PrimitiveTypeDecoder(bytes.ReadNextBytes(length));
				}
			}
		}
		public virtual object Decode(byte[] bytes, ConverterCategory converterCategory = ConverterCategory.ANY) {
			return Decode(new BinaryCodeReader(bytes), converterCategory);
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

		/// <summary>
		/// Local object used for preventing endless encoding loops
		/// </summary>
		private List<object> _encodedComplexObjects = null;
		protected virtual IEnumerable<byte> ComplexTypeEncoder(object o) {
			bool firstLevelEncoding = false;
			if(_encodedComplexObjects == null) {
				firstLevelEncoding = true;
				_encodedComplexObjects = new List<object>();
			}

			if (_encodedComplexObjects.Contains(o)) {
				// Infinite encoding loop detected
				throw new InfiniteEncodingLoopException(o, "Infinite encoding loop detected");
			}

			_encodedComplexObjects.Add(o); // Remember we already encoded this one object in particular

			List<byte> result = new List<byte>();
			Type t = o.GetType();

			// Original type accessable fields (all fields apart from private fields in base types as these are not inherited)
			var hierarchyFieldInfos = new List<FieldInfo[]>();
			hierarchyFieldInfos.Add(t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
			Type inherited = t.BaseType;

			// Base types non-public fields (later restricted to private fields only)
			while(inherited != null && inherited != typeof(object)) {
				hierarchyFieldInfos.Add(inherited.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));

				inherited = inherited.BaseType;
			}

			int hierarchyDescentCounter = 0; // How many times to go up the class parent hierarchy to get access to currently encoded private fields
			bool originalType = true; // whether we are currently encoding fields accessable from the object type, not parent types
			object currentlyEncodedObject;
			BinaryCodeBuilder encodedAsBuilder;
			BinaryCodeBuilder builder = new BinaryCodeBuilder();

			foreach (var fieldArr in hierarchyFieldInfos) {
				foreach(var field in fieldArr) {
					if(field.IsPrivate || originalType) {
						if(hierarchyDescentCounter > 0) {
							for(int i = 0; i < hierarchyDescentCounter; i++) {
								builder.AppendBit(1);
							}
							hierarchyDescentCounter = 0;
						}
						builder.AppendBit(0); // 0 indicates end of going up the class hierarchy

						byte[] bytes = EncodeFieldName(field.Name);
						builder.Append(EncodeLength((uint)bytes.Length));
						builder.AppendBytes(bytes);

						currentlyEncodedObject = field.GetValue(o);
						encodedAsBuilder = Encode(currentlyEncodedObject);
						builder.Append(encodedAsBuilder);
					}
				}

				hierarchyDescentCounter++;
				originalType = false;
			}

			if (firstLevelEncoding) {
				_encodedComplexObjects.Clear();
				_encodedComplexObjects = null;
			}

			return builder;
		}

		protected virtual object ComplexTypeDecoder(IEnumerable<byte> bytes) {
			Type currentType = CurrentDecodedType;
			object uninitialized = FormatterServices.GetUninitializedObject(currentType);
			BinaryCodeReader reader = new BinaryCodeReader(bytes);
			uint length;
			string fieldName;
			object value;

			while (!reader.EndOfSource()) {
				while(reader.ReadNextBit() == 1) {
					currentType = currentType.BaseType;

					if(currentType == null) {
						// TODO: throw valid exception
					}
				}

				length = DecodeLength(reader);
				fieldName = DecodeFieldName(reader.ReadNextBytes(length));
				var prop = currentType.GetField(
					fieldName, 
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
				);
				value = Decode(reader);
				prop.SetValue(uninitialized, value);
			}

			return uninitialized;
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
