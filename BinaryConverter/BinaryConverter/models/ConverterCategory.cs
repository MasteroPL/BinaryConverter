using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.models
{
	public enum ConverterCategory
	{
		/// <summary>
		/// Any converter tha can be found
		/// </summary>
		ANY = -1,
		/// <summary>
		/// 2. A custom converter added manually to the converters list within a class
		/// </summary>
		CUSTOM = 2,
		/// <summary>
		/// 3. A custom converter defined within the converter class
		/// </summary>
		CUSTOM_PREDEFINED = 3,
		/// <summary>
		/// 4. A custom converter defined within a converted type
		/// </summary>
		CUSTOM_FOR_CLASS = 4,
		/// <summary>
		/// 5. A custom converter defined globally for given class type
		/// </summary>
		GLOBAL_FOR_CLASS = 5,
		/// <summary>
		/// 6. A default converter added manually to the converters list within a class
		/// </summary>
		DEFAULT = 6,
		/// <summary>
		/// 7. A default converter defined within the converter class
		/// </summary>
		DEFAULT_PREDEFINED = 7,
		/// <summary>
		/// 8. A default converter defined for a serializable type
		/// </summary>
		SERIALIZABLE = 8,
		/// <summary>
		/// 9. A default converter (defined within the converter class)
		/// </summary>
		COMPLEX = 9
	}
}
