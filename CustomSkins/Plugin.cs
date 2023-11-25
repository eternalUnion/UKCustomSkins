using BepInEx;
using BepInEx.Bootstrap;
using CustomSkins.Managers;
using HarmonyLib;
using PluginConfig;
using System;
using Unity.Audio;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomSkins
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	[BepInDependency(PluginConfiguratorController.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
	public class Plugin : BaseUnityPlugin
	{
		public const string PLUGIN_NAME = "CustomSkins";
		public const string PLUGIN_GUID = "eternalUnion.CustomSkins";
		public const string PLUGIN_VERSION = "1.0.0";

		public const string PLUGIN_CONFIG_MIN_VER = "1.8.0";

		internal static AssetBundle bundle;

		private void Awake()
		{
			Paths.CreateAllPaths();
			
			// Check plugin config dependency
			if (!Chainloader.PluginInfos.ContainsKey(PluginConfiguratorController.PLUGIN_GUID))
			{
				Debug.LogError($"CustomSkins requires Plugin Configurator version {PLUGIN_CONFIG_MIN_VER} or above (current: not installed) to function correctly");
				enabled = false;
				return;
			}
			else if (Chainloader.PluginInfos[PluginConfiguratorController.PLUGIN_GUID].Metadata.Version < new Version(PLUGIN_CONFIG_MIN_VER))
			{
				Debug.LogError($"CustomSkins requires Plugin Configurator version {PLUGIN_CONFIG_MIN_VER} or above (current: {Chainloader.PluginInfos[PluginConfiguratorController.PLUGIN_GUID].Metadata.Version}) to function correctly");
				ConfigManager.ShowDependencyError();
				enabled = false;
				return;
			}

			PostAwake();
		}

		private static void PostAwake()
		{
			new Harmony(PLUGIN_GUID).PatchAll();

			Addressables.InitializeAsync().WaitForCompletion();
			AddressableAssetManager.Init();

			bundle = AssetBundle.LoadFromMemory(ManifestReader.GetBytes("AssetBundles.customskins"));

			ConfigManager.Init();
			ReloadManager.Init();

			ConfigManager.ScanSkins();
			SkinManager.ReloadAllSkins();

			Debug.Log($"Loaded {PLUGIN_NAME}");
		}
	}
}
