using CustomSkins.Managers.MaterialManagers;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Components
{
	internal class InternalComponents
	{
		public class WeaponMaterialReloadEventListener : MonoBehaviour
		{
			public Action onReload;

			public void OnReload()
			{
				onReload.Invoke();
			}

			private void Awake()
			{
				WeaponMaterialManager.onWeaponMaterialReload += OnReload;
			}

			private void OnDestroy()
			{
				WeaponMaterialManager.onWeaponMaterialReload -= OnReload;
			}
		}
	}
}
