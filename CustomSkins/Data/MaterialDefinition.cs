using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace CustomSkins.Data
{
	public class MaterialDefinition
	{
		/// <summary>
		/// Name of the material to change
		/// </summary>
		public string targetMaterial { get; set; }

		/// <summary>
		/// Name of the material to use as a base. It will be instantiated as a reference
		/// </summary>
		public string baseMaterial { get; set; }

		/// <summary>
		/// Where the base material is located. Default is local
		/// </summary>
		[DefaultValue(nameof(ResourceSource.addressables))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public ResourceSource baseMaterialSource { get; set; }

		/// <summary>
		/// Color of the material, in format `r,g,b` or `r,g,b,a` where components are between 0.0 and 1.0. Not set if null
		/// </summary>
		public string color { get; set; }

		/// <summary>
		/// Texture of the material, where the value is path to the texture. Default source is local files
		/// </summary>
		public string mainTexture { get; set; }

		/// <summary>
		/// Where the texture is located. Default is local
		/// </summary>
		[DefaultValue(nameof(ResourceSource.local))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public ResourceSource mainTextureSource { get; set; }
		
		/// <summary>
		/// Offset of the texture, in format `x,y`. Not sef if null
		/// </summary>
		public string mainTextureOffset { get; set; }

		/// <summary>
		/// Scale of the texture, in format `x,y`. Not sef if null
		/// </summary>
		public string mainTextureScale { get; set; }

		/// <summary>
		/// Address of the shader. This field will be used to load the shader
		/// </summary>
		public string shader { get; set; }

		[DefaultValue(nameof(WeaponVariationColored.@default))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public WeaponVariationColored variationColored { get; set; }

		/// <summary>
		/// Source of the shader. Addressables will load the shader from ultrakill assets. Asset bundle will load the shader from skin bundle.
		/// </summary>
		[DefaultValue(nameof(ResourceSource.addressables))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public ResourceSource shaderSource { get; set; }

		/// <summary>
		/// Shader property values to set
		/// </summary>
		public Dictionary<string, ShaderProperty> shaderProperties { get; set; }

		public MaterialFilter filters;
	}

	public enum WeaponVariationFilter
	{
		all,
		blue,
		green,
		red
	}

	public enum WeaponTypeFilter
	{
		all,
		stock,
		alt
	}

	public enum WeaponVariationColored
	{
		@default,
		yes,
		no
	}

	public class MaterialFilter
	{
		[DefaultValue(-1)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public int weaponNumber { get; set; }

		[DefaultValue(nameof(WeaponVariationFilter.all))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public WeaponVariationFilter weaponVariation { get; set; }

		[DefaultValue(nameof(WeaponTypeFilter.all))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public WeaponTypeFilter weaponType { get; set; }
	}

	public enum ShaderPropertyType
	{
		unknown = 0,
		color, // `r,g,b,a`
		colorArray, // `(r0,g0,b0,a0),(r1,g1,b1,a1),...`
		@float, // `0.0`
		floatArray, // `0.0,1.0,2.0`
		@int, // `0`
		texture, // `gun_text.png`
		textureOffset, // `0.0,0.0`
		textureScale, // `0.0,0.0`
		vector, // `x,y,z,w`
		vectorArray, // `(x0,y0,z0,w0),(x1,y1,z1,w1),...`
	}

	public class ShaderProperty
	{
		public ShaderPropertyType type { get; set; }
		public string value { get; set; }

		[DefaultValue(nameof(ResourceSource.local))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public ResourceSource valueSource { get; set; }

		public static bool TryDeserializeColor(string value, out Color color)
		{
			color = Color.white;

			if (string.IsNullOrEmpty(value))
				return false;

			string[] components = value.Split(',');
			if (components.Length == 3 || components.Length == 4)
			{
				if (!float.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float r))
					return false;
				if (!float.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float g))
					return false;
				if (!float.TryParse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float b))
					return false;

				float a = 1f;
				if (components.Length == 4)
					if (!float.TryParse(components[3], NumberStyles.Any, CultureInfo.InvariantCulture, out a))
						return false;

				color = new Color(r, g, b, a);
				return true;
			}
			else
			{
				return false;
			}
		}
	
		public static bool TryDeserializeColorArray(string value, out Color[] colorArr)
		{
			List<Color> colorList = new List<Color>();
			colorArr = null;

			if (string.IsNullOrEmpty(value))
			{
				colorArr = new Color[0];
				return true;
			}

			int openingPoint = value.IndexOf('(');
			if (openingPoint == -1)
				return false;

			int closingPoint = -1;

			while (openingPoint != -1)
			{
				closingPoint = value.IndexOf(')', openingPoint + 1);
				if (closingPoint == -1)
					return false;

				string colorString = value.Substring(openingPoint + 1, closingPoint - openingPoint - 1);
				if (TryDeserializeColor(colorString, out Color clr))
					colorList.Add(clr);
				else
					return false;

				openingPoint = value.IndexOf('(', closingPoint + 1);
			}

			colorArr = colorList.ToArray();
			return true;
		}
	
		public static bool TryDeserializeFloat(string value, out float flt)
		{
			if (string.IsNullOrEmpty(value))
			{
				flt = 0;
				return false;
			}

			return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out flt);
		}

		public static bool TryDeserializeInt(string value, out int i)
		{
			if (string.IsNullOrEmpty(value))
			{
				i = 0;
				return false;
			}

			return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out i);
		}

		public static bool TryDeserializeFloatArray(string value, out float[] arr)
		{
			List<float> floats = new List<float>();
			arr = null;

			if (string.IsNullOrEmpty(value))
			{
				arr = new float[0];
				return true;
			}

			foreach (var floatString in value.Split(','))
			{
				if (float.TryParse(floatString, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
					floats.Add(f);
				else
					return false;
			}

			arr = floats.ToArray();
			return true;
		}

		public static bool TryDeserializeVector(string value, out Vector4 vec)
		{
			vec = Vector4.zero;

			if (string.IsNullOrEmpty(value))
				return false;

			string[] components = value.Split(',');

			if (components.Length < 2 || components.Length > 4)
				return false;

			if (!float.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x))
				return false;
			if (!float.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
				return false;

			vec.x = x;
			vec.y = y;

			if (components.Length >= 3)
			{
				if (!float.TryParse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z))
					return false;

				vec.z = z;

				if (components.Length >= 4)
				{
					if (!float.TryParse(components[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float w))
						return false;

					vec.w = w;
				}
			}

			return true;
		}
		
		public static bool TryDeserializeVectorArray(string value, out Vector4[] vecArr)
		{
			List<Vector4> vecList = new List<Vector4>();
			vecArr = null;

			if (string.IsNullOrEmpty(value))
			{
				vecArr = new Vector4[0];
				return true;
			}

			int openingPoint = value.IndexOf('(');
			if (openingPoint == -1)
				return false;

			int closingPoint = -1;

			while (openingPoint != -1)
			{
				closingPoint = value.IndexOf(')', openingPoint + 1);
				if (closingPoint == -1)
					return false;

				string vectorString = value.Substring(openingPoint + 1, closingPoint - openingPoint - 1);
				if (TryDeserializeVector(vectorString, out Vector4 vec))
					vecList.Add(vec);
				else
					return false;

				openingPoint = value.IndexOf('(', closingPoint + 1);
			}

			vecArr = vecList.ToArray();
			return true;
		}
	}
}
