using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Managers.IconManager
{
	public static class WeaponIconManager
	{
		public delegate void OnIconReload();
		public static event OnIconReload onIconReload;

		public static WeaponIcon lastCallingIcon = null;

		public static void OnSkinReload()
		{
			lastCallingIcon = null;

			if (onIconReload != null)
				onIconReload.Invoke();

			if (lastCallingIcon != null)
				lastCallingIcon.UpdateIcon();
		}
	}
}
