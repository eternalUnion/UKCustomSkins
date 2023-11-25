using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomSkins
{
	public static class ManifestReader
	{
		public static byte[] GetBytes(string resourceName)
		{
			using (var str = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CustomSkins.Resources.{resourceName}"))
			{
				byte[] buff = new byte[str.Length];
				str.Read(buff, 0, buff.Length);
				return buff;
			}
		}

		public static Texture2D ReadTexture(string resourceName)
		{
			Texture2D image = new Texture2D(2, 2);
			image.LoadImage(ManifestReader.GetBytes(resourceName));
			return image;
		}

		public static Sprite ReadSprite(string resourceName)
		{
			Texture2D image = ReadTexture(resourceName);
			return Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f));
		}
	}
}
