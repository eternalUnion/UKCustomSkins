using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace CustomSkins.Utils
{
	public static class IOUtils
	{
		public static void TryCreateDirectory(string dir)
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public static string GetUniqueDirectory(string dir, string postfix = "_#", int startIndex = 0)
		{
			string parentDir = Path.GetDirectoryName(dir);
			string dirName = Path.GetFileName(dir);

			string newDirName = dir;
			while (Directory.Exists(newDirName))
				newDirName = Path.Combine(parentDir, $"{dirName}{postfix.Replace("#", (startIndex++).ToString(CultureInfo.InvariantCulture))}");

			return newDirName;
		}

		public static string GetUniqueFileName(string dir, string postfix = "_#", int startIndex = 0)
		{
			string parentDir = Path.GetDirectoryName(dir);
			string fileName = Path.GetFileNameWithoutExtension(dir);
			string fileExt = Path.GetExtension(dir);

			string newFileName = dir;
			while (File.Exists(newFileName))
				newFileName = Path.Combine(parentDir, $"{fileName}{postfix.Replace("#", (startIndex++).ToString(CultureInfo.InvariantCulture))}{fileExt}");

			return newFileName;
		}

		public static bool EndsInDirectorySeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			return path[path.Length - 1] == Path.DirectorySeparatorChar || path[path.Length - 1] == Path.AltDirectorySeparatorChar;
		}
	}
}
