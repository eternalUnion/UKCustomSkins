using CustomSkins.Components;
using CustomSkins.Data;
using CustomSkins.Managers.MaterialManagers;
using CustomSkins.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomSkins.Patches
{
	[HarmonyPatch(typeof(HookArm))]
	public static class HookArmPatches
	{
		[HarmonyPatch(nameof(HookArm.Start))]
		[HarmonyPostfix]
		public static void ModifyWhiplashMat(HookArm __instance)
		{
			SkinnedMeshRenderer armRenderer = __instance.gameObject.GetComponentInChildren<SkinnedMeshRenderer>(true);

			if (armRenderer != null)
			{
				Material defaultMaterial = armRenderer.material;
				string matName = UnityUtils.RemoveClonePostfix(defaultMaterial.name);

				void ReloadMaterial()
				{
					if (WeaponMaterialManager.TryGetWeaponMaterial(matName, 0, WeaponVariationFilter.green, WeaponTypeFilter.stock, out Material weaponMat, out MaterialDefinition weaponMatDef))
					{
						armRenderer.material = new Material(weaponMat);

						if (weaponMatDef.variationColored == WeaponVariationColored.yes)
						{
							if (armRenderer.material.HasProperty("_EmissiveColor"))
							{
								armRenderer.material.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[1]);
							}
							else
							{
								armRenderer.material.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[1];
							}
						}
					}
					else
					{
						armRenderer.material = new Material(defaultMaterial);
					}
				}

				InternalComponents.WeaponMaterialReloadEventListener reloadListener = armRenderer.gameObject.AddComponent<InternalComponents.WeaponMaterialReloadEventListener>();
				reloadListener.onReload = () =>
				{
					ReloadMaterial();
				};

				ReloadMaterial();
			}
		}
	}
}
