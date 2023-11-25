using CustomSkins.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomSkins.Providers
{
	// Base for a file source (from directory, zip, tar etc.)
	public abstract class FileProvider
	{
		public virtual string name { get => ""; }

		public virtual string fullPath { get => ""; }

		public virtual void Dispose() { }

		public virtual void Open() { }

		public virtual void Close() { }

		public abstract bool DirectoryExists(string dir);

		public abstract bool FileExists(string dir);

		public abstract IEnumerable<string> GetDirectories(string dir);

		public abstract IEnumerable<string> GetFiles(string dir);

		public abstract string ReadFile(string path);

		public abstract byte[] ReadBytes(string path);

		public abstract void WriteFile(string path, string content);

		public abstract void WriteFileBytes(string path, byte[] bytes);
	}

	public class DirectoryProvider : FileProvider
	{
		public readonly string path;

		public override string name
		{
			get => Path.GetFileName(path);
		}

		public override string fullPath
		{
			get => path;
		}

		public DirectoryProvider(string path)
		{
			this.path = path;
		}

		public override bool DirectoryExists(string dir)
		{
			return Directory.Exists(Path.Combine(path, dir));
		}

		public override bool FileExists(string dir)
		{
			return File.Exists(Path.Combine(path, dir));
		}

		public override IEnumerable<string> GetDirectories(string dir)
		{
			dir = Path.Combine(path, dir);
			if (!Directory.Exists(dir))
				return Enumerable.Empty<string>();

			return Directory.GetDirectories(Path.Combine(path, dir));
		}

		public override IEnumerable<string> GetFiles(string dir)
		{
			dir = Path.Combine(path, dir);
			if (!Directory.Exists(dir))
				return Enumerable.Empty<string>();

			return Directory.GetFiles(Path.Combine(path, dir));
		}

		public override byte[] ReadBytes(string filePath)
		{
			return File.ReadAllBytes(Path.Combine(path, filePath));
		}

		public override string ReadFile(string filePath)
		{
			return File.ReadAllText(Path.Combine(path, filePath));
		}

		public override void WriteFile(string filePath, string content)
		{
			File.WriteAllText(Path.Combine(path, filePath), content);
		}

		public override void WriteFileBytes(string filePath, byte[] bytes)
		{
			File.WriteAllBytes(Path.Combine(path, filePath), bytes);
		}
	}

	public class ZipProvider : FileProvider
	{
		public readonly string path;

		public override string name
		{
			get => Path.GetFileNameWithoutExtension(path);
		}

		public override string fullPath
		{
			get => path;
		}

		private ZipArchive archive;

		public override void Dispose()
		{
			Close();
		}

		public override void Open()
		{
			if (archive != null)
				Close();

			archive = new ZipArchive(File.Open(path, FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update);
		}

		public override void Close()
		{
			if (archive == null)
				return;

			try
			{
				archive.Dispose();
			}
			catch (Exception)
			{

			}

			archive = null;
		}

		public ZipProvider(string path)
		{
			this.path = path;
		}

		private static string ToDirectoryString(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			path = path.Replace('\\', '/');
			if (path[0] == '.')
			{
				path = path.Substring(1);
				if (string.IsNullOrEmpty(path))
					return string.Empty;
			}
			if (path[0] == '/')
			{
				path = path.Substring(1);
				if (string.IsNullOrEmpty(path))
					return string.Empty;
			}

			if (path[path.Length - 1] != '/')
				path += '/';

			return path;
		}

		private static string ToFileString(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			path = path.Replace('\\', '/');
			if (path[0] == '.')
			{
				path = path.Substring(1);
				if (string.IsNullOrEmpty(path))
					return string.Empty;
			}
			if (path[0] == '/')
			{
				path = path.Substring(1);
				if (string.IsNullOrEmpty(path))
					return string.Empty;
			}

			if (path[path.Length - 1] == '/')
				path = path.Substring(0, path.Length - 1);

			return path;
		}

		private static string ParentZipDirectory(string path)
		{
			int indexOfSeparator = path.LastIndexOf('/');
			if (indexOfSeparator == -1)
				return string.Empty;

			return path.Substring(0, indexOfSeparator);
		}

		public override bool DirectoryExists(string dir)
		{
			dir = ToDirectoryString(dir);
			if (string.IsNullOrEmpty(dir))
				return false;

			return archive.Entries.Where(e => e.FullName == dir).Any();
		}

		public override bool FileExists(string dir)
		{
			dir = ToFileString(dir);
			if (string.IsNullOrEmpty(dir))
				return false;

			return archive.Entries.Where(e => e.FullName == dir).Any();
		}

		public override IEnumerable<string> GetDirectories(string dir)
		{
			string fullPath = string.IsNullOrEmpty(dir) ? string.Empty : ToDirectoryString(dir);
			return archive.Entries.Where(e =>
			{
				if (string.IsNullOrEmpty(e.FullName) || e.FullName[e.FullName.Length - 1] != '/')
					return false;

				string parentPath = ParentZipDirectory(ParentZipDirectory(e.FullName));
				if (string.IsNullOrEmpty(parentPath))
				{
					return parentPath == fullPath;
				}
				else
				{
					if (parentPath[parentPath.Length - 1] != '/')
						parentPath += '/';

					return parentPath == fullPath;
				}
			}).Select(e => e.FullName);
		}

		public override IEnumerable<string> GetFiles(string dir)
		{
			string fullPath = string.IsNullOrEmpty(dir) ? string.Empty : ToDirectoryString(dir);
			return archive.Entries.Where(e =>
			{
				if (string.IsNullOrEmpty(e.FullName) || e.FullName[e.FullName.Length - 1] == '/')
					return false;

				string parentPath = ParentZipDirectory(e.FullName);
				if (string.IsNullOrEmpty(parentPath))
				{
					return parentPath == fullPath;
				}
				else
				{
					if (parentPath[parentPath.Length - 1] != '/')
						parentPath += '/';

					return parentPath == fullPath;
				}
			}).Select(e => e.FullName);
		}

		public override byte[] ReadBytes(string filePath)
		{
			filePath = ToFileString(filePath);

			using (var entry = archive.GetEntry(filePath).Open())
			{
				byte[] buff = new byte[entry.Length];
				entry.Read(buff, 0, buff.Length);
				return buff;
			}
		}

		public override string ReadFile(string filePath)
		{
			filePath = ToFileString(filePath);

			using (var reader = new StreamReader(archive.GetEntry(filePath).Open()))
			{
				return reader.ReadToEnd();
			}
		}

		public override void WriteFile(string filePath, string content)
		{
			WriteFileBytes(filePath, Encoding.UTF8.GetBytes(content));
		}

		public override void WriteFileBytes(string filePath, byte[] bytes)
		{
			filePath = ToFileString(filePath);

			var entry = archive.GetEntry(filePath);
			if (entry == null)
				entry = archive.CreateEntry(filePath);

			using (var stream = entry.Open())
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.SetLength(0);

				stream.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
