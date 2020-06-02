using BinaryConverter.attributes;
using BinaryConverter.exceptions;
using System;
using System.Collections.Generic;
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
	/// -> 9. A default converter (defined within the converter class)
	/// </summary>
	public class BinaryConverter {
		private List<BinaryEncoderMethod> CustomEncoders = new List<BinaryEncoderMethod>();
		private List<BinaryDecoderMethod> CustomDecoders = new List<BinaryDecoderMethod>();
		private List<BinaryEncoderMethod> DefaultEncoders = new List<BinaryEncoderMethod>();
		private List<BinaryDecoderMethod> DefaultDecoders = new List<BinaryDecoderMethod>();

		private BinaryEncoderMethod[] _predefinedCustomEncoders;
		private BinaryDecoderMethod[] _predefinedCustomDecoders;
		private BinaryEncoderMethod[] _predefinedDefaultEncoders;
		private BinaryDecoderMethod[] _predefinedDefaultDecoders;

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
		/// Attempts to find a predefined custom encoder for given type within defined ones
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
								return (IEnumerable<byte>)m.Invoke(o, [input]);
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
								return (IEnumerable<byte>)m.Invoke(null, [input]);
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

			// Step 4.

			throw new NotImplementedException();
		}
		#endregion

		protected virtual IEnumerable<byte> ComplexTypeEncoder(object o) {
			throw new NotImplementedException();
		}
	}
}
