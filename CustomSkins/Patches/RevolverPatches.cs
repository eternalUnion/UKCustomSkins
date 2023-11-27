using CustomSkins.Components;
using CustomSkins.Data;
using CustomSkins.Managers.MaterialManagers;
using CustomSkins.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomSkins.Patches
{
	[HarmonyPatch(typeof(Revolver))]
	public static class RevolverPatches
	{
		[HarmonyPatch(nameof(Revolver.Start))]
		[HarmonyPostfix]
		public static void ModifyRevolverHand(Revolver __instance)
		{
			SkinnedMeshRenderer rightArmRenderer = __instance.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(rend => rend.gameObject.name == "RightArm").FirstOrDefault();

			if (rightArmRenderer != null)
			{
				Material defaultMaterial = rightArmRenderer.material;
				string matName = UnityUtils.RemoveClonePostfix(defaultMaterial.name);

				void ReloadMaterial()
				{
					if (WeaponMaterialManager.TryGetWeaponMaterial(matName, 0, WeaponVariationFilter.blue, WeaponTypeFilter.stock, out Material weaponMat, out MaterialDefinition weaponMatDef))
					{
						rightArmRenderer.material = new Material(weaponMat);

						if (weaponMatDef.variationColored == WeaponVariationColored.yes)
						{
							if (rightArmRenderer.material.HasProperty("_EmissiveColor"))
							{
								rightArmRenderer.material.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[0]);
							}
							else
							{
								rightArmRenderer.material.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[0];
							}
						}
					}
					else
					{
						rightArmRenderer.material = defaultMaterial;
					}
				}

				InternalComponents.WeaponMaterialReloadEventListener reloadListener = rightArmRenderer.gameObject.AddComponent<InternalComponents.WeaponMaterialReloadEventListener>();
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
