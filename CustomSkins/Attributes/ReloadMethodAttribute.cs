using System;
using System.Collections.Generic;
using System.Text;

namespace CustomSkins.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ReloadMethodAttribute : Attribute
	{
	}
}
