﻿using CustomSkins.Components;
using CustomSkins.Data;
using CustomSkins.Managers.MaterialManagers;
using CustomSkins.Providers;
using CustomSkins.Utils;
using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomSkins.Patches
{
	[HarmonyPatch(typeof(GunColorGetter))]
	public static class GunColorGetterPatches
	{
		[HarmonyPatch(nameof(GunColorGetter.Awake))]
		[HarmonyPostfix]
		public static void SetupCustomMaterials(GunColorGetter __instance)
		{
			Material[] defaultMats = (Material[]) __instance.defaultMaterials.Clone();
			Material[] defaultColoredMats = (Material[])__instance.coloredMaterials.Clone();
			WeaponIcon weaponIcon = __instance.gameObject.GetComponentInParent<WeaponIcon>();
			Renderer thisRenderer = __instance.gameObject.GetComponent<Renderer>();
			bool originalIsVariationColored = (weaponIcon == null) ? false : weaponIcon.variationColoredRenderers.Contains(thisRenderer);

			bool useGeneralMaterial = true;
			int weaponNumber = __instance.weaponNumber;
			WeaponVariationFilter variation = WeaponVariationFilter.all;
			WeaponTypeFilter typeFilter = WeaponTypeFilter.all;

			// Revolver/AltRevolver
			if (weaponNumber == 1)
			{
				Revolver rev = __instance.gameObject.GetComponentInParent<Revolver>();
				if (rev != null)
				{
					useGeneralMaterial = false;

					if (rev.gunVariation == 0)
						variation = WeaponVariationFilter.blue;
					else if (rev.gunVariation == 1)
						variation = WeaponVariationFilter.green;
					else if (rev.gunVariation == 2)
						variation = WeaponVariationFilter.red;
					else
						useGeneralMaterial = true;

					if (rev.altVersion)
						typeFilter = WeaponTypeFilter.alt;
					else
						typeFilter = WeaponTypeFilter.stock;
				}
			}
			// Shotgun
			else if (weaponNumber == 2)
			{
				Shotgun sho = __instance.gameObject.GetComponentInParent<Shotgun>();
				if (sho != null)
				{
					useGeneralMaterial = false;

					if (sho.variation == 0)
						variation = WeaponVariationFilter.blue;
					else if (sho.variation == 1)
						variation = WeaponVariationFilter.green;
					else if (sho.variation == 2)
						variation = WeaponVariationFilter.red;
					else
						useGeneralMaterial = true;

					typeFilter = WeaponTypeFilter.stock;
				}
			}
			// Nailgun/SawLauncher
			else if (weaponNumber == 3)
			{
				Nailgun nai = __instance.gameObject.GetComponentInParent<Nailgun>();
				if (nai != null)
				{
					useGeneralMaterial = false;

					if (nai.variation == 0)
						variation = WeaponVariationFilter.blue;
					else if (nai.variation == 1)
						variation = WeaponVariationFilter.green;
					else if (nai.variation == 2)
						variation = WeaponVariationFilter.red;
					else
						useGeneralMaterial = true;

					if (nai.altVersion)
						typeFilter = WeaponTypeFilter.alt;
					else
						typeFilter = WeaponTypeFilter.stock;
				}
			}
			// Railcannon
			else if (weaponNumber == 4)
			{
				Railcannon rai = __instance.gameObject.GetComponentInParent<Railcannon>();
				if (rai != null)
				{
					useGeneralMaterial = false;

					if (rai.variation == 0)
						variation = WeaponVariationFilter.blue;
					else if (rai.variation == 1)
						variation = WeaponVariationFilter.green;
					else if (rai.variation == 2)
						variation = WeaponVariationFilter.red;
					else
						useGeneralMaterial = true;

					typeFilter = WeaponTypeFilter.stock;
				}
			}
			// RocketLauncher
			else if (weaponNumber == 5)
			{
				RocketLauncher rck = __instance.gameObject.GetComponentInParent<RocketLauncher>();
				if (rck != null)
				{
					useGeneralMaterial = false;

					if (rck.variation == 0)
						variation = WeaponVariationFilter.blue;
					else if (rck.variation == 1)
						variation = WeaponVariationFilter.green;
					else if (rck.variation == 2)
						variation = WeaponVariationFilter.red;
					else
						useGeneralMaterial = true;

					typeFilter = WeaponTypeFilter.stock;
				}
			}

			void ReloadMats()
			{
				// Priority list:
				// 1: Filtered weapon material (variation, alt/stock, weapon number => best match)
				// 2: General weapon material (no filter, material name match)
				// 3: Default weapon material

				bool usingCustomColor = __instance.GetPreset() != 0 || (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + __instance.weaponNumber + (__instance.altVersion ? ".a" : ""), false) && GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(__instance.weaponNumber - 1)));

				for (int i = 0; i < defaultMats.Length; i++)
				{
					string matName = UnityUtils.RemoveClonePostfix(defaultMats[i].name);

					if (useGeneralMaterial && WeaponMaterialManager.TryGetGeneralMaterial(matName, out Material generalMat, out MaterialDefinition generalMatDef))
					{
						__instance.defaultMaterials[i] = new Material(generalMat);

						if (i == 0 && !usingCustomColor && weaponIcon != null)
						{
							bool variationColored;
							if (generalMatDef.variationColored == WeaponVariationColored.@default)
								variationColored = originalIsVariationColored;
							else
								variationColored = generalMatDef.variationColored == WeaponVariationColored.yes;

							if (variationColored)
							{
								if (!weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.AddItem(thisRenderer).ToArray();
							}
							else
							{
								if (weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.Where(rend => rend != thisRenderer).ToArray();
							}
						}
					}
					else if (!useGeneralMaterial && WeaponMaterialManager.TryGetWeaponMaterial(matName, weaponNumber, variation, typeFilter, out Material weaponMat, out MaterialDefinition weaponMatDef))
					{
						__instance.defaultMaterials[i] = new Material(weaponMat);

						if (i == 0 && !usingCustomColor && weaponIcon != null)
						{
							bool variationColored;
							if (weaponMatDef.variationColored == WeaponVariationColored.@default)
								variationColored = originalIsVariationColored;
							else
								variationColored = weaponMatDef.variationColored == WeaponVariationColored.yes;

							if (variationColored)
							{
								if (!weaponIcon.variationColoredRenderers.Contains(thisRenderer))
								{
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.AddItem(thisRenderer).ToArray();
								}
							}
							else
							{
								if (weaponIcon.variationColoredRenderers.Contains(thisRenderer))
								{
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.Where(rend => rend != thisRenderer).ToArray();
								}
							}
						}
					}
					else
					{
						__instance.defaultMaterials[i] = new Material(defaultMats[i]);

						if (i == 0 && !usingCustomColor && weaponIcon != null)
						{
							if (originalIsVariationColored)
							{
								if (!weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.AddItem(thisRenderer).ToArray();
							}
							else
							{
								if (weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.Where(rend => rend != thisRenderer).ToArray();
							}
						}
					}
				}

				for (int i = 0; i < defaultColoredMats.Length; i++)
				{
					string matName = UnityUtils.RemoveClonePostfix(defaultColoredMats[i].name);

					if (useGeneralMaterial && WeaponMaterialManager.TryGetGeneralMaterial(matName, out Material generalMat, out MaterialDefinition generalMatDef))
					{
						__instance.coloredMaterials[i] = new Material(generalMat);

						if (i == 0 && usingCustomColor && weaponIcon != null)
						{
							bool variationColored;
							if (generalMatDef.variationColored == WeaponVariationColored.@default)
								variationColored = originalIsVariationColored;
							else
								variationColored = generalMatDef.variationColored == WeaponVariationColored.yes;

							if (variationColored)
							{
								if (!weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.AddItem(thisRenderer).ToArray();
							}
							else
							{
								if (weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.Where(rend => rend != thisRenderer).ToArray();
							}
						}
					}
					else if (!useGeneralMaterial && WeaponMaterialManager.TryGetWeaponMaterial(matName, weaponNumber, variation, typeFilter, out Material weaponMat, out MaterialDefinition weaponMatDef))
					{
						__instance.coloredMaterials[i] = new Material(weaponMat);

						if (i == 0 && usingCustomColor && weaponIcon != null)
						{
							bool variationColored;
							if (weaponMatDef.variationColored == WeaponVariationColored.@default)
								variationColored = originalIsVariationColored;
							else
								variationColored = weaponMatDef.variationColored == WeaponVariationColored.yes;

							if (variationColored)
							{
								if (!weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.AddItem(thisRenderer).ToArray();
							}
							else
							{
								if (weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.Where(rend => rend != thisRenderer).ToArray();
							}
						}
					}
					else
					{
						__instance.coloredMaterials[i] = new Material(defaultColoredMats[i]);

						if (i == 0 && usingCustomColor && weaponIcon != null)
						{
							if (originalIsVariationColored)
							{
								if (!weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.AddItem(thisRenderer).ToArray();
							}
							else
							{
								if (weaponIcon.variationColoredRenderers.Contains(thisRenderer))
									weaponIcon.variationColoredRenderers = weaponIcon.variationColoredRenderers.Where(rend => rend != thisRenderer).ToArray();
							}
						}
					}
				}
			}

			InternalComponents.WeaponMaterialReloadEventListener listener = __instance.gameObject.AddComponent<InternalComponents.WeaponMaterialReloadEventListener>();
			listener.onReload = () =>
			{
				ReloadMats();

				__instance.UpdateColor();
			};
			listener.Subscribe();

			ReloadMats();
		}
	}
}
