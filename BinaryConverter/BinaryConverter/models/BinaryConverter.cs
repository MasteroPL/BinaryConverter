using BinaryConverter.attributes;
using BinaryConverter.exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		protected byte[] EncodeClassName(string className) {
			return Encoding.UTF8.GetBytes(className);
		}

		protected string DecodeClassName(byte[] bytes) {
			return Encoding.UTF8.GetString(bytes);
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
		protected virtual Func<object, IEnumerable<byte>> GetEncoder(object o) {
			Type objectType = o.GetType();
			BinaryEncoderMethod encoder;
			Func<object, IEnumerable<byte>> encoderMethod;

			// Step 1. Looking for a custom converter added manually to the converters list
			encoder = GetCustomEncoder(objectType);
			if(encoder != null) {
				// Found the appropriate encoder
				return encoder.Encoder;
			}

			//  Step 2. Looking for a custom converter defined within the converter class
			encoder = GetPredefinedCustomEncoder(objectType);
			if(encoder != null) {
				// Found the appropriate encoder
				return encoder.Encoder;
			}

			// Step 3. Looking for a custom converter defined within the converted class
			encoderMethod = GetCustomClassEncoder(o);
			if(encoderMethod != null) {
				return encoderMethod;
			}

			// Step 4. Looking for a global converter defined for the class
			encoderMethod = GetGlobalClassEncoder(objectType);
			if(encoderMethod != null) {
				return encoderMethod;
			}

			// Step 5. Looking for a default converter added manually to the converters list
			encoder = GetDefaultEncoder(objectType);
			if(encoder != null) {
				return encoder.Encoder;
			}

			// Step 6. Looking for a default converter defined within the converter class
			encoder = GetPredefinedDefaultEncoder(objectType);
			if(encoder != null) {
				return encoder.Encoder;
			}

			// Step 7. Attempting a converter for a primitive type
			if (objectType.IsSerializable) {
				return SerializableTypeEncoder;
			}

			// Step 8. None of the above is valid, apply the default encoder
			return ComplexTypeEncoder;
		}

		/// <summary>
		/// Finds the appriopriate encoder within specified category
		/// </summary>
		/// <param name="o">Object to find the encoder for</param>
		/// <param name="category">Category of encoders to consider</param>
		/// <returns>An encoder method appropriate for that object or null if not found</returns>
		protected Func<object, IEnumerable<byte>> GetSpecificEncoder(object o, ConverterCategory category) {
			BinaryEncoderMethod encoder;
			switch (category) {
				case ConverterCategory.ANY:
					return GetEncoder(o);

				case ConverterCategory.CUSTOM:
					encoder = GetCustomEncoder(o.GetType());
					return (encoder == null) ? null : encoder.Encoder;

				case ConverterCategory.CUSTOM_PREDEFINED:
					encoder = GetPredefinedCustomEncoder(o.GetType());
					return (encoder == null) ? null : encoder.Encoder;

				case ConverterCategory.CUSTOM_FOR_CLASS:
					return GetCustomClassEncoder(o);

				case ConverterCategory.GLOBAL_FOR_CLASS:
					return GetGlobalClassEncoder(o.GetType());

				case ConverterCategory.DEFAULT:
					encoder = GetDefaultEncoder(o.GetType());
					return (encoder == null) ? null : encoder.Encoder;

				case ConverterCategory.DEFAULT_PREDEFINED:
					encoder = GetPredefinedDefaultEncoder(o.GetType());
					return (encoder == null) ? null : encoder.Encoder;

				case ConverterCategory.SERIALIZABLE:
					if (o.GetType().IsSerializable) {
						return SerializableTypeEncoder;
					}
					return null;

				case ConverterCategory.COMPLEX:
					if (!o.GetType().IsPrimitive) {
						return ComplexTypeEncoder;
					}
					return null;

				default:
					throw new NotImplementedException("Provided category was not implemented");
			}
		}
		#endregion

		public virtual IEnumerable<byte> Encode(object o, ConverterCategory converterCategory = ConverterCategory.ANY) {
			byte[] classNameBytes = EncodeClassName(o.GetType().FullName);

			Func<object, IEnumerable<byte>> encoder = GetSpecificEncoder(o, converterCategory);
			IEnumerable<byte> encodedObject = encoder.Invoke(o);

			int length = classNameBytes.Length + encodedObject.Count();

			throw new NotImplementedException();
		}

		protected virtual IEnumerable<byte> SerializableTypeEncoder(object o) {
			MemoryStream ms = new MemoryStream();
			_binaryFormatter.Serialize(ms, o);

			return ms.ToArray();
		}
		protected virtual object SerializableTypeDecoder(IEnumerable<byte> bytes) {
			MemoryStream ms = new MemoryStream();
			ms.Write(bytes.ToArray(), 0, bytes.Count());
			ms.Seek(0, SeekOrigin.Begin);
			return _binaryFormatter.Deserialize(ms);
		}

		protected virtual IEnumerable<byte> ComplexTypeEncoder(object o) {
			throw new NotImplementedException();
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
