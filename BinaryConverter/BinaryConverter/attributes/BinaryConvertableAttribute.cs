using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryConverter.attributes
{
	/// <summary>
	/// The purpose of this attribute is strictly for deterministic reasons.
	/// It's used to indicate, that a given class was prepared for Binary Conversions
	/// 
	/// In case of given class not being marked with this attribute and BinaryConverter being in "STRICT" mode, the conversion will not happen
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	class BinaryConvertableAttribute : Attribute { }
}
