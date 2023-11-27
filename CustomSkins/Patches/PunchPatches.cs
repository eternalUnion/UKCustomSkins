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
	[HarmonyPatch(typeof(Punch))]
	public static class PunchPatches
	{
		[HarmonyPatch(nameof(Punch.Start))]
		[HarmonyPostfix]
		public static void ModifyPunchMat(Punch __instance)
		{
			SkinnedMeshRenderer armRenderer = __instance.gameObject.GetComponentInChildren<SkinnedMeshRenderer>(true);

			if (armRenderer != null)
			{
				Material defaultMaterial = armRenderer.material;
				string matName = UnityUtils.RemoveClonePostfix(defaultMaterial.name);

				void ReloadMaterial()
				{
					WeaponVariationFilter variation;
					int variationColor;
					if (__instance.type == FistType.Standard)
					{
						variation = WeaponVariationFilter.blue;
						variationColor = 0;
					}
					else if (__instance.type == FistType.Heavy)
					{
						variation = WeaponVariationFilter.red;
						variationColor = 2;
					}
					else
					{
						variation = WeaponVariationFilter.green;
						variationColor = 1;
					}

					if (WeaponMaterialManager.TryGetWeaponMaterial(matName, 0, variation, WeaponTypeFilter.stock, out Material weaponMat, out MaterialDefinition weaponMatDef))
					{
						armRenderer.material = new Material(weaponMat);

						if (weaponMatDef.variationColored == WeaponVariationColored.yes)
						{
							if (armRenderer.material.HasProperty("_EmissiveColor"))
							{
								armRenderer.material.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
							}
							else
							{
								armRenderer.material.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor];
							}
						}
					}
					else
					{
						armRenderer.material = defaultMaterial;
					}
				}

				InternalComponents.WeaponMaterialReloadEventListener reloadListener = armRenderer.gameObject.AddComponent<InternalComponents.WeaponMaterialReloadEventListener>();
				reloadListener.onReload = () =>
				{
					ReloadMaterial();
				};
				reloadListener.Subscribe();

				ReloadMaterial();
			}
		}
	}
}
