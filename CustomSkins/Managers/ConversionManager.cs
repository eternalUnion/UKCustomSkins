using CustomSkins.Data;
using CustomSkins.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CustomSkins.Managers
{
	public static class ConversionManager
	{
		public static class UltraskinsConverter
		{
			const string VERTEXLIT_EM_SHADER = "Assets/Shaders/Main/ULTRAKILL-vertexlit-emissive.shader";
			const string VERTEXLIT_EM_CUSTOM_SHADER = "Assets/Shaders/Special/ULTRAKILL-vertexlit-customcolors-emissive.shader";
			const string VERTEXLIT_RAI_EM_SHADER = "Assets/Shaders/Special/ULTRAKILL-vertexlit-railgun.shader";
			const string VERTEXLIT_RAI_EM_CUSTOM_SHADER = "Assets/Shaders/Special/ULTRAKILL-vertexlit-customcolors-railgun.shader";

			const string ARM_BLU_TEX_FILENAME = "ArmV2_Diffuse.png";
			const string ARM_BLU_EM_FILENAME = "ArmV2_Diffuse_Emissive.png";

			const string ARM_RED_TEX_FILENAME = "v2_armtex.png";
			const string ARM_RED_EM_FILENAME = "v2_armtex_Emissive.png";

			const string ARM_GRE_TEX_FILENAME = "T_GreenArm.png";
			const string ARM_GRE_EM_FILENAME = "T_GreenArm_Emissive.png";

			const string REV_TEX_FILENAME = "T_PistolNew.png";
			const string REV_EM_FILENAME = "T_PistolNew_Emissive.png";
			const string REV_ID_FILENAME = "T_PistolNew_ID.png";

			const string REV_ALT_TEX_FILENAME = "T_MinosRevolver_128.png";
			const string REV_ALT_EM_FILENAME = "T_MinosRevolver_128_Emissive.png";
			const string REV_ALT_ID_FILENAME = "T_MinosRevolver_ID.png";

			const string SHO_TEX_FILENAME = "T_ShotgunNew.png";
			const string SHO_EM_FILENAME = "T_ShotgunNew_Emissive.png";
			const string SHO_ID_FILENAME = "T_ShotgunNew_ID.png";

			const string NAI_TEX_FILENAME = "T_NailgunNew_NoGlow.png";
			const string NAI_EM_FILENAME = "T_Nailgun_New_Glow.png";
			const string NAI_ID_FILENAME = "T_NailgunNew_ID.png";

			const string SWL_TEX_FILENAME = "T_SawbladeLauncher.png";
			const string SWL_EM_FILENAME = "T_SawbladeLauncher_Emissive.png";
			const string SWL_ID_FILENAME = "T_SawbladeLauncher_ID.png";

			const string RAI_TEX_FILENAME = "Railgun_Main_AlphaGlow.png";
			const string RAI_EM_FILENAME = "Railgun_Main_Emissive.png";
			const string RAI_ID_FILENAME = "T_Railgun_ID.png";

			const string RCK_TEX_FILENAME = "T_RocketLauncher.png";
			const string RCK_EM_FILENAME = "T_RocketLauncher_Emissive.png";
			const string RCK_ID_FILENAME = "T_RocketLauncher_ID.png";

			public static Exception ProcessUltraskinsFolder(string path)
			{
				byte[] blankEmission = ManifestReader.GetBytes("blank-emission.png");

				string skinName = Path.GetFileName(path);
				string destinationDirectory = IOUtils.GetUniqueDirectory(Path.Combine(Paths.SkinsFolder, skinName));
				Directory.CreateDirectory(destinationDirectory);

				try
				{
					// Logo
					File.WriteAllBytes(Path.Combine(destinationDirectory, "icon.png"), ManifestReader.GetBytes("ultraskins-logo.png"));

					// Create skin info
					File.WriteAllText(Path.Combine(destinationDirectory, "info.json"), JsonConvert.SerializeObject(SkinInfo.CreateNewInfo(skinName, "Unknown"), Formatting.Indented));

					string dataPath = Path.Combine(destinationDirectory, "Data");
					Directory.CreateDirectory(dataPath);
					string materialsPath = Path.Combine(dataPath, "Materials");
					Directory.CreateDirectory(materialsPath);
					string texturesPath = Path.Combine(destinationDirectory, "Textures");
					Directory.CreateDirectory(texturesPath);

					void ProcessSkin(string textureName, string emissionName, string idName, string materialName, string materialAddress, string customMaterialName, string idAddress, string materialDataName, string customColorMaterialDataName, string shader = VERTEXLIT_EM_SHADER, string customShader = VERTEXLIT_EM_CUSTOM_SHADER)
					{
						string texturePath = Path.Combine(path, textureName);
						string emissionPath = Path.Combine(path, emissionName);
						string idPath = string.IsNullOrEmpty(idName) ? null : Path.Combine(path, idName);

						bool textureExists = File.Exists(texturePath);
						bool emissionExists = File.Exists(emissionPath);
						bool idExists = string.IsNullOrEmpty(idName) ? false : File.Exists(idPath);

						// Skin undefined
						if (!textureExists && !emissionExists && !idExists)
							return;

						MaterialDefinition matDef = MaterialDefinition.Create();

						matDef.targetMaterial = materialName;
						matDef.baseMaterial = materialAddress;
						matDef.shader = shader;
						matDef.variationColored = WeaponVariationColored.yes;
						matDef.shaderProperties = new Dictionary<string, ShaderProperty>();

						if (textureExists)
						{
							File.Copy(texturePath, Path.Combine(texturesPath, textureName), true);
							matDef.mainTexture = $"Textures/{textureName}";
						}

						if (emissionExists)
							File.Copy(emissionPath, Path.Combine(texturesPath, emissionName), true);
						else
							File.WriteAllBytes(Path.Combine(texturesPath, emissionName), blankEmission);

						if (emissionName == RAI_EM_FILENAME)
						{
							matDef.mainTextureAlpha = $"Textures/{emissionName}";
							matDef.variationColored = WeaponVariationColored.no;
						}
						else
						{
							matDef.shaderProperties["_EmissiveTex"] = new ShaderProperty()
							{
								type = ShaderPropertyType.texture,
								value = $"Textures/{emissionName}",
								valueSource = ResourceSource.local
							};
						}

						File.WriteAllText(Path.Combine(materialsPath, materialDataName), JsonConvert.SerializeObject(matDef, Formatting.Indented));

						if (!string.IsNullOrEmpty(idAddress))
						{
							matDef.targetMaterial = customMaterialName;
							matDef.baseMaterial = idAddress;
							matDef.shader = customShader;

							if (idExists)
							{
								File.Copy(idPath, Path.Combine(texturesPath, idName), true);
								matDef.shaderProperties["_IDTex"] = new ShaderProperty()
								{
									type = ShaderPropertyType.texture,
									value = $"Textures/{idName}",
									valueSource = ResourceSource.local
								};
							}

							File.WriteAllText(Path.Combine(materialsPath, customColorMaterialDataName), JsonConvert.SerializeObject(matDef, Formatting.Indented));
						}
					}

					ProcessSkin(ARM_BLU_TEX_FILENAME, ARM_BLU_EM_FILENAME, null, "Arm", "Assets/Materials/Arm/Arm.mat", null, null, "arm_blue.json", null);
					ProcessSkin(ARM_RED_TEX_FILENAME, ARM_RED_EM_FILENAME, null, "RedArmLit", "Assets/Models/V1/Arms/RedArmLit.mat", null, null, "arm_red.json", null);
					ProcessSkin(ARM_GRE_TEX_FILENAME, ARM_GRE_EM_FILENAME, null, "GreenArm", "Assets/Models/V1/Arms/GreenArm.mat", null, null, "arm_green.json", null);

					ProcessSkin(REV_TEX_FILENAME, REV_EM_FILENAME, REV_ID_FILENAME, "Pistol New", "Assets/Models/Weapons/Revolver/Pistol New.mat", "Pistol New CustomColor", "Assets/Models/Weapons/Revolver/Pistol New CustomColor.mat", "revolver.json", "revolver_custom.json");
					
					ProcessSkin(REV_ALT_TEX_FILENAME, REV_ALT_EM_FILENAME, REV_ALT_ID_FILENAME, "MinosRevolver", "Assets/Models/Weapons/Alternative Revolver/MinosRevolver.mat", "MinosRevolver CustomColor", "Assets/Models/Weapons/Alternative Revolver/MinosRevolver CustomColor.mat", "revolver_alt.json", "revolver_alt_custom.json");

					ProcessSkin(SHO_TEX_FILENAME, SHO_EM_FILENAME, SHO_ID_FILENAME, "Shotgun New", "Assets/Models/Weapons/Shotgun/Shotgun New.mat", "Shotgun New CustomColor", "Assets/Models/Weapons/Shotgun/Shotgun New CustomColor.mat", "shotgun.json", "shotgun_custom.json");

					ProcessSkin(NAI_TEX_FILENAME, NAI_EM_FILENAME, NAI_ID_FILENAME, "Nailgun New", "Assets/Models/Weapons/Nailgun/Nailgun New.mat", "Nailgun New CustomColor", "Assets/Models/Weapons/Nailgun/Nailgun New CustomColor.mat", "nailgun.json", "nailgun_custom.json");

					ProcessSkin(SWL_TEX_FILENAME, SWL_EM_FILENAME, SWL_ID_FILENAME, "SawbladeLauncher", "Assets/Models/Weapons/Sawblade Launcher/SawbladeLauncher.mat", "SawbladeLauncher CustomColor", "Assets/Models/Weapons/Sawblade Launcher/SawbladeLauncher CustomColor.mat", "sawblade_launcher.json", "sawblade_launcher_custom.json");

					ProcessSkin(RAI_TEX_FILENAME, RAI_EM_FILENAME, RAI_ID_FILENAME, "Railcannon", "Assets/Models/Weapons/Railcannon/Railcannon.mat", "Railcannon CustomColor", "Assets/Models/Weapons/Railcannon/Railcannon CustomColor.mat", "railcannon_blue.json", "railcannon_custom.json", shader: VERTEXLIT_RAI_EM_SHADER, customShader: VERTEXLIT_RAI_EM_CUSTOM_SHADER);
					ProcessSkin(RAI_TEX_FILENAME, RAI_EM_FILENAME, null, "RailcannonHarpoon", "Assets/Models/Weapons/Railcannon/RailcannonHarpoon.mat", null, null, "railcannon_green.json", null, shader: VERTEXLIT_RAI_EM_SHADER, customShader: VERTEXLIT_RAI_EM_CUSTOM_SHADER);
					ProcessSkin(RAI_TEX_FILENAME, RAI_EM_FILENAME, null, "RailcannonMalicious", "Assets/Models/Weapons/Railcannon/RailcannonMalicious.mat", null, null, "railcannon_red.json", null, shader: VERTEXLIT_RAI_EM_SHADER, customShader: VERTEXLIT_RAI_EM_CUSTOM_SHADER);

					ProcessSkin(RCK_TEX_FILENAME, RCK_EM_FILENAME, RCK_ID_FILENAME, "RocketLauncher", "Assets/Models/Weapons/RocketLauncher/RocketLauncher.mat", "RocketLauncherCustom", "Assets/Models/Weapons/RocketLauncher/RocketLauncherCustom.mat", "rocket_launcher.json", "rocket_launcher_custom.json");
				}
				catch (Exception e)
				{
					Debug.LogError("Failed to convert ultraskins folder");
					Debug.LogException(e);
					if (Directory.Exists(destinationDirectory))
						Directory.Delete(destinationDirectory, true);

					return e;
				}

				return null;
			}
		}
	}
}
