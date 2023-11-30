using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

	public static class SpriteExtensions
	{
		public static Sprite CreateSpriteFrom(this Texture2D texture)
		{
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}
	}
}
