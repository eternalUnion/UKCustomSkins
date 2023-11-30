using CustomSkins.Components;
using CustomSkins.Data;
using CustomSkins.Managers.MaterialManagers;
using CustomSkins.Utils;
using HarmonyLib;
using Sandbox.Arm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomSkins.Patches
{
	[HarmonyPatch(typeof(SandboxArm))]
	public static class SandboxArmPatches
	{
		[HarmonyPatch(nameof(SandboxArm.Awake))]
		[HarmonyPostfix]
		public static void ModifySandboxArmMat(HookArm __instance)
		{
			SkinnedMeshRenderer armRenderer = __instance.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(rend => rend.gameObject.name == "RightArm").FirstOrDefault();

			if (armRenderer != null)
			{
				Material defaultMaterial = armRenderer.material;
				string matName = UnityUtils.RemoveClonePostfix(defaultMaterial.name);

				void ReloadMaterial()
				{
					if (__instance == null)
					{
						WeaponMaterialManager.onWeaponMaterialReload -= ReloadMaterial;
						return;
					}

					if (WeaponMaterialManager.TryGetWeaponMaterial(matName, 0, WeaponVariationFilter.blue, WeaponTypeFilter.stock, out Material weaponMat, out MaterialDefinition weaponMatDef))
					{
						armRenderer.material = new Material(weaponMat);

						if (weaponMatDef.variationColored == WeaponVariationColored.yes)
						{
							if (armRenderer.material.HasProperty("_EmissiveColor"))
							{
								armRenderer.material.SetColor("_EmissiveColor", Color.white);
							}
							else
							{
								armRenderer.material.color = Color.white;
							}
						}
					}
					else
					{
						armRenderer.material = defaultMaterial;
					}
				}

				WeaponMaterialManager.onWeaponMaterialReload += ReloadMaterial;
				ReloadMaterial();
			}
		}
	}
}
