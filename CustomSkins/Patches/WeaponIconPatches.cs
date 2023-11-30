using CustomSkins.Components;
using CustomSkins.Managers;
using CustomSkins.Managers.IconManager;
using CustomSkins.Providers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Patches
{
	[HarmonyPatch(typeof(WeaponIcon))]
	public static class WeaponIconPatches
	{
		private class WeaponIconTracker : MonoBehaviour
		{
			public WeaponDescriptor defaultDescriptor = null;
			public WeaponDescriptor currentDescriptor = null;

			private void OnDestroy()
			{
				if (currentDescriptor != null && currentDescriptor != defaultDescriptor)
					Destroy(currentDescriptor);
			}
		}

		[HarmonyPatch(nameof(WeaponIcon.OnEnable))]
		[HarmonyPrefix]
		public static bool ModifyIcon(WeaponIcon __instance)
		{
			if (__instance.weaponDescriptor == null)
				return true;

			if (__instance.gameObject.GetComponent<WeaponIconTracker>() != null)
				return true;

			string iconName = __instance.weaponDescriptor.weaponName;
			WeaponIconTracker tracker = __instance.gameObject.AddComponent<WeaponIconTracker>();
			tracker.defaultDescriptor = __instance.weaponDescriptor;

			void ReloadIcon()
			{
				if (__instance == null)
				{
					WeaponIconManager.onIconReload -= ReloadIcon;
					return;
				}

				__instance.weaponDescriptor = tracker.defaultDescriptor;

				if (SkinManager.TryGetIcon(iconName, out IconInstance iconInstance))
				{
					if (tracker.currentDescriptor == null)
						tracker.currentDescriptor = UnityEngine.Object.Instantiate(tracker.defaultDescriptor);

					tracker.currentDescriptor.icon = iconInstance.icon;
					tracker.currentDescriptor.glowIcon = iconInstance.iconGlow;
					__instance.weaponDescriptor = tracker.currentDescriptor;
				}

				__instance.UpdateIcon();

				if (__instance.gameObject.activeInHierarchy)
					WeaponIconManager.lastCallingIcon = __instance;
			}

			WeaponIconManager.onIconReload += ReloadIcon;
			ReloadIcon();
			return true;
		}
	}
}
