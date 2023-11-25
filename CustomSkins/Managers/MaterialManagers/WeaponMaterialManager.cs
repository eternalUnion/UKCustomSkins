using CustomSkins.Attributes;
using CustomSkins.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Managers.MaterialManagers
{
	public static class WeaponMaterialManager
	{
		public delegate void OnWeaponMaterialReload();
		public static event OnWeaponMaterialReload onWeaponMaterialReload;

		/*private static Dictionary<string, Material> customGeneralMaterial = new Dictionary<string, Material>();

		private static Dictionary<string, Material>[][] customStockMaterials;

		private static Dictionary<string, Material>[][] customAltMaterials;*/

		private static Dictionary<string, Tuple<Material, MaterialDefinition>> customGeneralMaterials = new Dictionary<string, Tuple<Material, MaterialDefinition>>();
		private static Dictionary<string, Tuple<Material, MaterialDefinition>>[,,] customMaterials = new Dictionary<string, Tuple<Material, MaterialDefinition>>[6, 3, 2];

		static WeaponMaterialManager()
		{
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						customMaterials[i, j, k] = new Dictionary<string, Tuple<Material, MaterialDefinition>>();
					}
				}
			}
		}

		private static void ClearAllMaterials()
		{
			customGeneralMaterials.Clear();

			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						customMaterials[i, j, k].Clear();
					}
				}
			}
		}

		private static int VariationToIndex(WeaponVariationFilter var)
		{
			switch (var)
			{
				case WeaponVariationFilter.all:
					throw new ArgumentException();

				case WeaponVariationFilter.blue:
					return 0;

				case WeaponVariationFilter.green:
					return 1;

				case WeaponVariationFilter.red:
					return 2;

				default:
					throw new ArgumentException();
			}
		}

		private static void TryLoadWeaponMaterial(string materialName, int weaponNum, WeaponVariationFilter variation, WeaponTypeFilter type)
		{
			if (SkinManager.TryGetWeaponMaterial(materialName, weaponNum, variation, type, out Material weaponMat, out MaterialDefinition matDef))
			{
				customMaterials[weaponNum, VariationToIndex(variation), (type == WeaponTypeFilter.stock ? 0 : 1)][materialName] = new Tuple<Material, MaterialDefinition>(weaponMat, matDef);
			}
		}

		private static void TryLoadGeneralMaterial(string materialName)
		{
			if (SkinManager.TryGetGeneralWeaponMaterial(materialName, out var weaponMat, out var weaponMatDef))
			{
				customGeneralMaterials[materialName] = new Tuple<Material, MaterialDefinition>(weaponMat, weaponMatDef);
			}
		}

		internal static bool TryGetWeaponMaterial(string materialName, int weaponNum, WeaponVariationFilter variation, WeaponTypeFilter type, out Material mat, out MaterialDefinition matDef)
		{
			if (customMaterials[weaponNum, VariationToIndex(variation), (type == WeaponTypeFilter.stock ? 0 : 1)].TryGetValue(materialName, out var weaponMat))
			{
				mat = weaponMat.Item1;
				matDef = weaponMat.Item2;

				return true;
			}

			mat = null;
			matDef = null;
			return false;
		}

		internal static bool TryGetGeneralMaterial(string materialName, out Material mat, out MaterialDefinition matDef)
		{
			if (customGeneralMaterials.TryGetValue(materialName, out var weaponMat))
			{
				mat = weaponMat.Item1;
				matDef = weaponMat.Item2;
				return true;
			}

			mat = null;
			matDef = null;
			return false;
		}

		[ReloadMethod]
		public static void OnSkinReload()
		{
			Debug.Log("Reloading weapon skins");

			ClearAllMaterials();

			// Pistol
			TryLoadGeneralMaterial("Pistol New");
			TryLoadGeneralMaterial("Pistol New CustomColor");

			TryLoadWeaponMaterial("Pistol New", 1, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New", 1, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New", 1, WeaponVariationFilter.red, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New CustomColor", 1, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("Pistol New CustomColor", 1, WeaponVariationFilter.green, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("Pistol New CustomColor", 1, WeaponVariationFilter.red, WeaponTypeFilter.alt);

			if (onWeaponMaterialReload != null)
				onWeaponMaterialReload.Invoke();
		}
	}
}
