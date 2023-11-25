using CustomSkins.Data;
using CustomSkins.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomSkins.Managers
{
	public static class SkinManager
	{
		private static List<AssetProvider> _providers = new List<AssetProvider>();

		private static void DisposeAllAssetProviders()
		{
			foreach (var prov in _providers)
				prov.Dispose();
			_providers.Clear();
		}

		public static void ReloadAllSkins()
		{
			DisposeAllAssetProviders();

			foreach (string skinId in ConfigManager.enabledSkinOrder)
			{
				string path = ConfigManager.TryGetSkinPath(skinId);
				if (string.IsNullOrEmpty(path))
					continue;

				if (Directory.Exists(path))
				{
					_providers.Add(new AssetProvider(new DirectoryProvider(path)));
				}
				else if (File.Exists(path))
				{
					if (path.EndsWith(".zip"))
						_providers.Add(new AssetProvider(new ZipProvider(path)));
					else
						Debug.LogWarning($"Could not load skin at {path} due to unknown file type");
				}
			}

			ReloadManager.ReloadAll();
		}

		public static bool TryGetMaterial(string name, out Material mat, out MaterialDefinition matDef)
		{
			foreach (var provider in _providers)
			{
				if (provider.TryGetMaterial(name, out mat, out matDef))
					return true;
			}

			mat = null;
			matDef = null;
			return false;
		}

		public static bool TryGetGeneralWeaponMaterial(string name, out Material mat, out MaterialDefinition matDef)
		{
			foreach (var provider in _providers)
			{
				if (provider.TryGetGeneralWeaponMaterial(name, out mat, out matDef))
					return true;
			}

			mat = null;
			matDef = null;
			return false;
		}

		public static bool TryGetWeaponMaterial(string name, int weaponNumber, WeaponVariationFilter weaponVariation, WeaponTypeFilter weaponType, out Material mat, out MaterialDefinition matDef)
		{
			foreach (var provider in _providers)
			{
				if (provider.TryGetWeaponMaterial(name, weaponNumber, weaponVariation, weaponType, out mat, out matDef))
					return true;
			}

			mat = null;
			matDef = null;
			return false;
		}
	}
}
