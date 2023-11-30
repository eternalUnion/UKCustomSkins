using CustomSkins.Data;
using CustomSkins.Providers;
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

		private static Dictionary<string, MaterialInstance> customGeneralMaterials = new Dictionary<string, MaterialInstance>();
		private static Dictionary<string, MaterialInstance>[,,] customMaterials = new Dictionary<string, MaterialInstance>[6, 3, 2];

		static WeaponMaterialManager()
		{
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						customMaterials[i, j, k] = new Dictionary<string, MaterialInstance>();
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
				customMaterials[weaponNum, VariationToIndex(variation), (type == WeaponTypeFilter.stock ? 0 : 1)][materialName] = new MaterialInstance(matDef, weaponMat);
			}
		}

		private static void TryLoadGeneralMaterial(string materialName)
		{
			if (SkinManager.TryGetGeneralWeaponMaterial(materialName, out var weaponMat, out var weaponMatDef))
			{
				customGeneralMaterials[materialName] = new MaterialInstance(weaponMatDef, weaponMat);
			}
		}

		internal static bool TryGetWeaponMaterial(string materialName, int weaponNum, WeaponVariationFilter variation, WeaponTypeFilter type, out Material mat, out MaterialDefinition matDef)
		{
			if (customMaterials[weaponNum, VariationToIndex(variation), (type == WeaponTypeFilter.stock ? 0 : 1)].TryGetValue(materialName, out var weaponMat))
			{
				mat = weaponMat.material;
				matDef = weaponMat.materialDefinition;

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
				mat = weaponMat.material;
				matDef = weaponMat.materialDefinition;
				return true;
			}

			mat = null;
			matDef = null;
			return false;
		}

		public static void OnSkinReload()
		{
			Debug.Log("Reloading weapon skins");

			ClearAllMaterials();

			// Arms
			TryLoadGeneralMaterial("Arm White");
			TryLoadWeaponMaterial("Arm White", 0, WeaponVariationFilter.blue, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Arm");
			TryLoadWeaponMaterial("Arm", 0, WeaponVariationFilter.blue, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RedArm");
			TryLoadWeaponMaterial("RedArm", 0, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RedArmLit");
			TryLoadWeaponMaterial("RedArmLit", 0, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("GreenArm");
			TryLoadWeaponMaterial("GreenArm", 0, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("GreenArmUnlit");
			TryLoadWeaponMaterial("GreenArmUnlit", 0, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			// Revolver
			TryLoadGeneralMaterial("Pistol New");
			TryLoadWeaponMaterial("Pistol New", 1, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New", 1, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New", 1, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Pistol New CustomColor");
			TryLoadWeaponMaterial("Pistol New CustomColor", 1, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New CustomColor", 1, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New CustomColor", 1, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Pistol New Unlit");
			TryLoadWeaponMaterial("Pistol New Unlit", 1, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New Unlit", 1, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New Unlit", 1, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Pistol New Unlit CustomColor");
			TryLoadWeaponMaterial("Pistol New Unlit CustomColor", 1, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New Unlit CustomColor", 1, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Pistol New Unlit CustomColor", 1, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			// Alt Revolver
			TryLoadGeneralMaterial("MinosRevolver");
			TryLoadWeaponMaterial("MinosRevolver", 1, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver", 1, WeaponVariationFilter.green, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver", 1, WeaponVariationFilter.red, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("MinosRevolver CustomColor");
			TryLoadWeaponMaterial("MinosRevolver CustomColor", 1, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver CustomColor", 1, WeaponVariationFilter.green, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver CustomColor", 1, WeaponVariationFilter.red, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("MinosRevolver Unlit");
			TryLoadWeaponMaterial("MinosRevolver Unlit", 1, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver Unlit", 1, WeaponVariationFilter.green, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver Unlit", 1, WeaponVariationFilter.red, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("MinosRevolver Unlit CustomColor");
			TryLoadWeaponMaterial("MinosRevolver Unlit CustomColor", 1, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver Unlit CustomColor", 1, WeaponVariationFilter.green, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("MinosRevolver Unlit CustomColor", 1, WeaponVariationFilter.red, WeaponTypeFilter.alt);

			// Shotgun
			TryLoadGeneralMaterial("Shotgun New");
			TryLoadWeaponMaterial("Shotgun New", 2, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Shotgun New", 2, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Shotgun New CustomColor");
			TryLoadWeaponMaterial("Shotgun New CustomColor", 2, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Shotgun New CustomColor", 2, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Shotgun New Unlit");
			TryLoadWeaponMaterial("Shotgun New Unlit", 2, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Shotgun New Unlit", 2, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Shotgun New Unlit CustomColor");
			TryLoadWeaponMaterial("Shotgun New Unlit CustomColor", 2, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Shotgun New Unlit CustomColor", 2, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			// Nailgun
			TryLoadGeneralMaterial("Nailgun New");
			TryLoadWeaponMaterial("Nailgun New", 3, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Nailgun New", 3, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Nailgun New CustomColor");
			TryLoadWeaponMaterial("Nailgun New CustomColor", 3, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Nailgun New CustomColor", 3, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Nailgun New Unlit");
			TryLoadWeaponMaterial("Nailgun New Unlit", 3, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Nailgun New Unlit", 3, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Nailgun New Unlit CustomColor");
			TryLoadWeaponMaterial("Nailgun New Unlit CustomColor", 3, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Nailgun New Unlit CustomColor", 3, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			// SawLauncher
			TryLoadGeneralMaterial("Sawblade");
			TryLoadWeaponMaterial("Sawblade", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("Sawblade", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("Sawblade CustomColor");
			TryLoadWeaponMaterial("Sawblade CustomColor", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("Sawblade CustomColor", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("Sawblade Unlit");
			TryLoadWeaponMaterial("Sawblade Unlit", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("Sawblade Unlit", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("Sawblade Unlit CustomColor");
			TryLoadWeaponMaterial("Sawblade Unlit CustomColor", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("Sawblade Unlit CustomColor", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("SawbladeLauncher");
			TryLoadWeaponMaterial("SawbladeLauncher", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("SawbladeLauncher", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("SawbladeLauncher CustomColor");
			TryLoadWeaponMaterial("SawbladeLauncher CustomColor", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("SawbladeLauncher CustomColor", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("SawbladeLauncher Unlit");
			TryLoadWeaponMaterial("SawbladeLauncher Unlit", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("SawbladeLauncher Unlit", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			TryLoadGeneralMaterial("SawbladeLauncher Unlit CustomColor");
			TryLoadWeaponMaterial("SawbladeLauncher Unlit CustomColor", 3, WeaponVariationFilter.blue, WeaponTypeFilter.alt);
			TryLoadWeaponMaterial("SawbladeLauncher Unlit CustomColor", 3, WeaponVariationFilter.green, WeaponTypeFilter.alt);

			// Railcannon
			TryLoadGeneralMaterial("Railcannon");
			TryLoadWeaponMaterial("Railcannon", 4, WeaponVariationFilter.blue, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RailcannonHarpoon");
			TryLoadWeaponMaterial("RailcannonHarpoon", 4, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RailcannonMalicious");
			TryLoadWeaponMaterial("RailcannonMalicious", 4, WeaponVariationFilter.red, WeaponTypeFilter.stock);
			
			TryLoadGeneralMaterial("Railcannon CustomColor");
			TryLoadWeaponMaterial("Railcannon CustomColor", 4, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Railcannon CustomColor", 4, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Railcannon CustomColor", 4, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Railcannon Unlit");
			TryLoadWeaponMaterial("Railcannon Unlit", 4, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Railcannon Unlit", 4, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Railcannon Unlit", 4, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("Railcannon Unlit CustomColor");
			TryLoadWeaponMaterial("Railcannon Unlit CustomColor", 4, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Railcannon Unlit CustomColor", 4, WeaponVariationFilter.green, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("Railcannon Unlit CustomColor", 4, WeaponVariationFilter.red, WeaponTypeFilter.stock);

			// RocketLauncher
			TryLoadGeneralMaterial("RocketLauncher");
			TryLoadWeaponMaterial("RocketLauncher", 5, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("RocketLauncher", 5, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RocketLauncherCustom");
			TryLoadWeaponMaterial("RocketLauncherCustom", 5, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("RocketLauncherCustom", 5, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RocketLauncher Unlit");
			TryLoadWeaponMaterial("RocketLauncher Unlit", 5, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("RocketLauncher Unlit", 5, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			TryLoadGeneralMaterial("RocketLauncher Unlit CustomColor");
			TryLoadWeaponMaterial("RocketLauncher Unlit CustomColor", 5, WeaponVariationFilter.blue, WeaponTypeFilter.stock);
			TryLoadWeaponMaterial("RocketLauncher Unlit CustomColor", 5, WeaponVariationFilter.green, WeaponTypeFilter.stock);

			if (onWeaponMaterialReload != null)
				onWeaponMaterialReload.Invoke();
		}
	}
}
