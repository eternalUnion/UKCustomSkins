using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CustomSkins.Data
{
	public class SkinInfo
	{
		[DefaultValue("Unnamed Skin")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public string skinName { get; set; }

		[DefaultValue("Unknown")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public string author { get; set; }

		public string uniqueIdentifier { get; set; }

		[DefaultValue("1.0.0")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public string skinVersion { get; set; }

		[DefaultValue(Plugin.PLUGIN_VERSION)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public string customSkinsMinimumVersion { get; set; }

		public static SkinInfo CreateNewInfo(string skinName, string author)
		{
			return new SkinInfo()
			{
				skinName = skinName,
				author = author,
				uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", ""),
				skinVersion = "1.0.0",
				customSkinsMinimumVersion = Plugin.PLUGIN_VERSION
			};
		}
	}
}
