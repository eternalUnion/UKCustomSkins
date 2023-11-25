using CustomSkins.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CustomSkins
{
	public static class Paths
	{
		static Paths()
		{

		}

		internal static void CreateAllPaths()
		{
			IOUtils.TryCreateDirectory(SkinsFolder);
		}

		public static string SkinsFolder => Path.Combine(BepInEx.Paths.ConfigPath, "CustomSkins");
	}
}
