using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	/// <summary>
	/// Class type used for converting objects into their binary representations and the other way around - recovers original objects (to a defined degree) from it's binary representation
	/// 
	/// Converters (Encoders and Decoders) for given type are used based on following prioritization (top to bottom):
	/// -> A custom converter provided as argument for conversion
	/// -> A custom converter defined within a converted type
	/// -> A custom converter defined globally for given class type
	/// -> A custom converter added manually to the converters list within a class
	/// -> A custom converter defined within the converter class
	/// -> A default converter (defined within the converter class)
	/// </summary>
	public class BinaryConverter
	{

	}
}
