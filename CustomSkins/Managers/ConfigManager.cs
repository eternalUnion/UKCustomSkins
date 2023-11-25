using CustomSkins.Data;
using CustomSkins.Fields;
using CustomSkins.Providers;
using CustomSkins.Utils;
using Newtonsoft.Json;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using PluginConfig.API.Functionals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomSkins.Managers
{
    public static class ConfigManager
    {
        private static PluginConfigurator config;
        private static bool dirty = false;

        private static PluginConfigurator internalConfig;
        private static List<string> enabledSkins = new List<string>();
        private static StringMultilineField enabledSkinsField;

        private static Dictionary<string, CustomSkinField> skinFields = new Dictionary<string, CustomSkinField>();
		private static Dictionary<string, CustomSkinField> currentSkinFields = new Dictionary<string, CustomSkinField>();
        private static Dictionary<string, string> idToPath = new Dictionary<string, string>();

        internal static string TryGetSkinPath(string id)
        {
            if (idToPath.TryGetValue(id, out string path))
                return path;
            return string.Empty;
        }

        internal static IReadOnlyCollection<string> enabledSkinOrder => enabledSkins;

        private static ConfigDivision skinFieldDiv;
        private static ConfigHeader enabledSkinsHeader;
        private static ConfigHeader disabledSkinsHeader;

		internal static void ShowDependencyError()
        {
			config = PluginConfigurator.Create("Custom Skins", Plugin.PLUGIN_GUID);

            var err = new ConfigHeader(config.rootPanel, $"Custom Skins requires version {Plugin.PLUGIN_CONFIG_MIN_VER} or above");
            err.textColor = Color.red;
		}

        private static void InitInternal()
        {
            if (internalConfig != null)
                return;

            internalConfig = PluginConfigurator.Create("CustomSkins_InternalConfig", $"{Plugin.PLUGIN_GUID}_internal");
            internalConfig.hidden = true;
            internalConfig.interactable = false;

            enabledSkinsField = new StringMultilineField(internalConfig.rootPanel, "", "enabledSkinsField", "", true, true);
            enabledSkins = enabledSkinsField.value.Split('\n').ToList();
        }

        private static void ReorderEnabledSkins()
        {
            int uiIndex = 1;
            foreach (string id in enabledSkins)
            {
                if (currentSkinFields.TryGetValue(id, out CustomSkinField enabledField))
                    enabledField.siblingIndex = uiIndex++;
            }
        }

        private static void ProcessSkin(FileProvider provider)
        {
            try
            {
                provider.Open();

                string infoFile = "info.json";
				if (!provider.FileExists(infoFile))
				{
					Debug.LogWarning($"Info file missing at {infoFile}, creating a new one");
                    provider.WriteFile(infoFile, JsonConvert.SerializeObject(SkinInfo.CreateNewInfo(Path.GetFileName(provider.name), Steamworks.SteamClient.IsValid ? Steamworks.SteamClient.Name : "Unknown")));
				}

				SkinInfo info = null;
				try
				{
					info = JsonConvert.DeserializeObject<SkinInfo>(provider.ReadFile(infoFile));
				}
				catch (Exception e)
				{
					Debug.LogError($"Exception thrown while reading skin info, skipping {provider.name}");
					Debug.LogException(e);
                    return;
				}

				bool rewriteInfo = false;

				if (!Version.TryParse(info.skinVersion, out _))
				{
					Debug.LogWarning($"Invalid skin version at {provider.name}, overwriting");
					info.skinVersion = "1.0.0";
					rewriteInfo = true;
				}

				if (!Version.TryParse(info.customSkinsMinimumVersion, out _))
				{
					Debug.LogWarning($"Invalid custom skins minimum version at {provider.name}, overwriting");
					info.customSkinsMinimumVersion = Plugin.PLUGIN_VERSION;
					rewriteInfo = true;
				}

				if (string.IsNullOrEmpty(info.uniqueIdentifier))
				{
					Debug.LogWarning($"Empty unique identifier at {provider.name}, overwriting");
					info.uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "");
					rewriteInfo = true;
				}
                else if (currentSkinFields.ContainsKey(info.uniqueIdentifier))
                {
					Debug.LogWarning($"Duplicate unique identifier at {provider.name}, overwriting");
                    do
                    {
                        info.uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "");
                    }
                    while (currentSkinFields.ContainsKey(info.uniqueIdentifier));
					rewriteInfo = true;
				}

				if (rewriteInfo)
				{
					try
					{
						provider.WriteFile(infoFile, JsonConvert.SerializeObject(info));
					}
					catch (Exception e)
					{
						Debug.LogError($"Exception thrown while rewriting skin info, skipping {provider.name}");
						Debug.LogException(e);
						return;
					}
				}

                if (!skinFields.TryGetValue(info.uniqueIdentifier, out CustomSkinField skinField))
                {
                    skinField = new CustomSkinField(skinFieldDiv);
                    skinFields[info.uniqueIdentifier] = skinField;

                    string uniqueIdentifier = info.uniqueIdentifier;
                    skinField.onEnableButton = () =>
                    {
                        if (enabledSkins.Contains(uniqueIdentifier))
                        {
                            enabledSkins.Remove(uniqueIdentifier);
							enabledSkinsField.value = string.Join("\n", enabledSkins);
							dirty = true;

                            skinField.movable = false;
                            skinField.enableButtonText = "+";

                            if (skinField.siblingIndex > disabledSkinsHeader.siblingIndex)
                                skinField.siblingIndex = disabledSkinsHeader.siblingIndex + 1;
                            else
								skinField.siblingIndex = disabledSkinsHeader.siblingIndex;
						}
                        else
                        {
                            enabledSkins.Add(uniqueIdentifier);
							enabledSkinsField.value = string.Join("\n", enabledSkins);
							dirty = true;

							skinField.movable = true;
							skinField.enableButtonText = "-";

							if (skinField.siblingIndex > disabledSkinsHeader.siblingIndex)
								skinField.siblingIndex = disabledSkinsHeader.siblingIndex;
							else
								skinField.siblingIndex = disabledSkinsHeader.siblingIndex - 1;
						}
                    };

                    skinField.onUpButton = () =>
                    {
                        int currentIndex = enabledSkins.IndexOf(uniqueIdentifier);

                        // How did we get here?
                        if (currentIndex == -1)
                        {
							dirty = true;

							skinField.movable = false;
							skinField.enableButtonText = "+";
							skinField.siblingIndex = disabledSkinsHeader.siblingIndex + 1;
						}
                        else if (currentIndex != 0)
                        {
                            enabledSkins.RemoveAt(currentIndex);
                            enabledSkins.Insert(currentIndex - 1, uniqueIdentifier);
							enabledSkinsField.value = string.Join("\n", enabledSkins);
							dirty = true;

                            ReorderEnabledSkins();
						}
                    };

					skinField.onDownButton = () =>
					{
						int currentIndex = enabledSkins.IndexOf(uniqueIdentifier);

						// How did we get here?
						if (currentIndex == -1)
						{
							dirty = true;

							skinField.movable = false;
							skinField.enableButtonText = "+";
							skinField.siblingIndex = disabledSkinsHeader.siblingIndex;
						}
						else if (currentIndex < enabledSkins.Count - 1)
						{
							enabledSkins.RemoveAt(currentIndex);
							enabledSkins.Insert(currentIndex + 1, uniqueIdentifier);
							enabledSkinsField.value = string.Join("\n", enabledSkins);
							dirty = true;

							ReorderEnabledSkins();
						}
					};
				}

                currentSkinFields[info.uniqueIdentifier] = skinField;

                skinField.skinName = info.skinName;
                skinField.authorName = info.author;
                skinField.showWarning = new Version(Plugin.PLUGIN_VERSION) < new Version(info.customSkinsMinimumVersion);

                if (provider.FileExists("icon.png"))
                {
                    try
                    {
                        byte[] iconBytes = provider.ReadBytes("icon.png");

                        var md5 = MD5.Create();
                        byte[] newHash = md5.ComputeHash(iconBytes);

                        if (skinField.iconHash == null || !Enumerable.SequenceEqual(newHash, skinField.iconHash))
                        {
                            Texture2D oldIcon = skinField.icon;

                            Texture2D newIcon = new Texture2D(2, 2);
                            newIcon.LoadImage(iconBytes);
                            skinField.icon = newIcon;
                            skinField.iconHash = newHash;

                            if (oldIcon != null)
                                UnityEngine.Object.Destroy(oldIcon);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Exception thrown while loading icon from {provider.name}");
                        Debug.LogException(e);
                    }
                }

                skinField.hidden = false;
                idToPath[info.uniqueIdentifier] = provider.fullPath;
			}
            catch (Exception e)
            {
                Debug.LogError("Caught exception while processing skin");
                Debug.LogException(e);
            }
            finally
            {
                provider.Dispose();
            }
		}

        public static void ScanSkins()
        {
            idToPath.Clear();
			currentSkinFields.Clear();

            foreach (var skinField in skinFields.Values)
            {
                skinField.hidden = true;
                skinField.movable = false;
                skinField.enableButtonText = "<color=lime>+</color>";
            }

			foreach (var folder in Directory.GetDirectories(Paths.SkinsFolder).OrderBy(d => new DirectoryInfo(d).CreationTime.Second))
                ProcessSkin(new DirectoryProvider(folder));

            foreach (var file in Directory.GetFiles(Paths.SkinsFolder).OrderBy(f => new FileInfo(f).CreationTime.Second))
            {
                if (file.EndsWith(".zip"))
                    ProcessSkin(new ZipProvider(file));
                else
                    Debug.LogWarning($"Unknown file type '{Path.GetExtension(file)}', skipping");
            }

            // Put all missing skin fields at the bottom
            foreach (var missingField in skinFields.Where(field => !currentSkinFields.ContainsKey(field.Key)).Select(field => field.Value))
                missingField.siblingIndex = skinFieldDiv.fieldCount - 1;

            // Put disabled fields under the disabled header if they are above the header for some reason
            foreach (var disabledField in currentSkinFields.Where(field => !enabledSkins.Contains(field.Key)).OrderBy(field => field.Value.siblingIndex).Select(field => field.Value))
            {
				disabledField.movable = false;
				disabledField.enableButtonText = "+";
                if (disabledField.siblingIndex < disabledSkinsHeader.siblingIndex)
                    disabledField.siblingIndex = disabledSkinsHeader.siblingIndex;
			}

            int uiIndex = 1;
            bool enabledListChanged = false;

			for (int i = enabledSkins.Count - 1; i >= 0; i--)
            {
				if (string.IsNullOrEmpty(enabledSkins[i]) || !currentSkinFields.TryGetValue(enabledSkins[i], out _))
				{
					enabledListChanged = true;
					enabledSkins.RemoveAt(i);
				}
			}

			for (int i = 0; i < enabledSkins.Count; i++)
            {
				if (currentSkinFields.TryGetValue(enabledSkins[i], out var enabledField))
                {
				    enabledField.movable = true;
                    enabledField.enableButtonText = "-";
                    enabledField.siblingIndex = uiIndex++;
                }
            }

            if (enabledListChanged)
            {
                dirty = true;
				enabledSkinsField.value = string.Join("\n", enabledSkins);
            }
        }

        internal static void Init()
        {
            if (config != null)
                return;

            InitInternal();

            config = PluginConfigurator.Create("Custom Skins", Plugin.PLUGIN_GUID);
            config.presetButtonHidden = true;
            config.rootPanel.onPannelCloseEvent += () =>
            {
                if (dirty)
                {
                    SkinManager.ReloadAllSkins();
                    dirty = false;
                }
            };

            try
            {
                config.icon = ManifestReader.ReadSprite("icon.png");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{Plugin.PLUGIN_NAME}] Error caught while loading icon from manifest:\n{e}");
            }

			// Placeholders
			var onlinePanel = new ConfigPanel(config.rootPanel, "Online Skins", "onlineSkins", ConfigPanel.PanelFieldType.StandardWithIcon);
			try
			{
				onlinePanel.icon = ManifestReader.ReadSprite("online-icon.png");
			}
			catch (Exception e)
			{
				Debug.LogError($"[{Plugin.PLUGIN_NAME}] Error caught while loading icon from manifest:\n{e}");
			}

			var convertPanel = new ConfigPanel(config.rootPanel, "Converter", "converter", ConfigPanel.PanelFieldType.StandardWithIcon);
			try
			{
				convertPanel.icon = ManifestReader.ReadSprite("convert-icon.png");
			}
			catch (Exception e)
			{
				Debug.LogError($"[{Plugin.PLUGIN_NAME}] Error caught while loading icon from manifest:\n{e}");
			}

			var topButtons = new ButtonArrayField(config.rootPanel, "topButtons", 2, new float[] { 0.5f, 0.5f }, new string[] { "Skins Folder", "Create New Skin" });
            topButtons.buttonHeight = 50;

            topButtons.OnClickEventHandler(0).onClick += () =>
            {
                Application.OpenURL(Paths.SkinsFolder);
            };

            topButtons.OnClickEventHandler(1).onClick += () =>
            {
                string destinationDir = IOUtils.GetUniqueDirectory(Path.Combine(Paths.SkinsFolder, "NewSkin"));
                Directory.CreateDirectory(destinationDir);

                string texturesFolder = Path.Combine(destinationDir, "Textures");
                Directory.CreateDirectory(texturesFolder);

				string[] TEXTURE_ADDRESSES = new string[]
				{
                    // Revolver
                    "Assets/Models/Weapons/Revolver/T_PistolNew.png",
					"Assets/Models/Weapons/Revolver/T_PistolNew_Emissive.png",
					"Assets/Models/Weapons/Revolver/T_PistolNew_ID.tga",

                    // Alt Revolver
					"Assets/Models/Weapons/Alternative Revolver/T_MinosRevolver_128.png",
                    "Assets/Models/Weapons/Alternative Revolver/T_MinosRevolver_128_Emissive.png",
					"Assets/Models/Weapons/Alternative Revolver/T_MinosRevolver_ID.tga",

                    // Shotgun
                    "Assets/Models/Weapons/Shotgun/T_ShotgunNew.png",
					"Assets/Models/Weapons/Shotgun/T_ShotgunNew_ID.tga",

                    // Nailgun
                    "Assets/Models/Weapons/Nailgun/T_NailgunNew_NoGlow.png",
					"Assets/Models/Weapons/Nailgun/T_Nailgun_New_Glow.png",
					"Assets/Models/Weapons/Nailgun/T_NailgunNew_ID.tga",
                    
                    // Saw
                    "Assets/Models/Weapons/Sawblade Launcher/T_SawbladeLauncher.png",
					"Assets/Models/Weapons/Sawblade Launcher/T_SawbladeLauncher_ID.tga",
					"Assets/Models/Weapons/Sawblade Launcher/T_Sawblade.png",
					"Assets/Models/Weapons/Sawblade Launcher/T_Sawblade_ID.tga",

                    // Railcannon
                    "Assets/Models/Weapons/Railcannon/Railgun_Main_AlphaGlow.tga",
					"Assets/Models/Weapons/Railcannon/T_Railgun_ID.tga",

                    // Rocket launcher
                    "Assets/Models/Weapons/RocketLauncher/T_RocketLauncher_ID.tga",
					"Assets/Models/Weapons/RocketLauncher/T_RocketLauncher_Desaturated.png",
					"Assets/Models/Weapons/RocketLauncher/T_RocketLauncher_ID.tga",
				};

                foreach (string textureAddress in TEXTURE_ADDRESSES)
                {
                    string fileName = Path.GetFileName(textureAddress);
                    fileName = Path.ChangeExtension(fileName, "png");

                    Texture2D texture = Addressables.LoadAssetAsync<Texture2D>(textureAddress).WaitForCompletion();
                    if (texture != null)
                    {
                        var duplicate = TextureUtils.DuplicateTexture(texture);
						File.WriteAllBytes(Path.Combine(texturesFolder, fileName), duplicate.EncodeToPNG());
                        UnityEngine.Object.Destroy(duplicate);
                    }
                }

                Application.OpenURL(destinationDir);
			};

            var scanButton = new ButtonField(config.rootPanel, "Scan For New Skin Files", "refreshButton");
            scanButton.buttonHeight = 50;
            
            scanButton.onClick += ScanSkins;

			var reloadButton = new ButtonField(config.rootPanel, "Force Reload All Skins", "reloadSkins");
			reloadButton.buttonHeight = 50;

			reloadButton.onClick += () =>
            {
                dirty = false;
                ScanSkins();
				SkinManager.ReloadAllSkins();
            };

			skinFieldDiv = new ConfigDivision(config.rootPanel, "skinFieldDiv");
            enabledSkinsHeader = new ConfigHeader(skinFieldDiv, "Enabled Skins", 20);
            enabledSkinsHeader.textColor = Color.green;
			disabledSkinsHeader = new ConfigHeader(skinFieldDiv, "Disabled Skins", 20);
			disabledSkinsHeader.textColor = Color.red;
		}
    }
}
