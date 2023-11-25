using System;
using System.Collections.Generic;
using System.Text;

namespace CustomSkins.Utils
{
	public static class UnityUtils
	{
		public static string RemoveClonePostfix(string name)
		{
			while (name.EndsWith(" (Instance)"))
				name = name.Substring(0, name.Length - " (Instance)".Length);

			if (name.EndsWith("(Clone)"))
				name = name.Substring(0, name.Length - "(Clone)".Length);

			return name;
		}
	}
}
