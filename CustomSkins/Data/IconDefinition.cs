using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace CustomSkins.Data
{
	public class IconDefinition
	{
		public string targetIcon { get; set; }

		public string iconTexture { get; set; }

		[DefaultValue(nameof(ResourceSource.local))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		[JsonConverter(typeof(StringEnumConverter))]
		public ResourceSource iconSource { get; set; }

		[DefaultValue(nameof(FilterMode.Bilinear))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		[JsonConverter(typeof(StringEnumConverter))]
		public FilterMode iconFilterMode { get; set; }

		public string iconGlowTexture { get; set; }

		[DefaultValue(nameof(ResourceSource.local))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		[JsonConverter(typeof(StringEnumConverter))]
		public ResourceSource iconGlowSource { get; set; }

		[DefaultValue(nameof(FilterMode.Bilinear))]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		[JsonConverter(typeof(StringEnumConverter))]
		public FilterMode iconGlowFilterMode { get; set; }
	}
}
