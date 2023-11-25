﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Utils
{
	public static class TextureUtils
	{
		// For unreadable textures
		// https://forum.unity.com/threads/easy-way-to-make-texture-isreadable-true-by-script.1141915/
		public static Texture2D DuplicateTexture(Texture2D source)
		{
			RenderTexture renderTex = RenderTexture.GetTemporary(
						source.width,
						source.height,
						0,
						RenderTextureFormat.Default,
						RenderTextureReadWrite.Linear);

			Graphics.Blit(source, renderTex);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D readableText = new Texture2D(source.width, source.height);
			readableText.filterMode = source.filterMode;
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}
	}
}
