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
			private bool subscribed = false;

			public void OnReload()
			{
				onReload.Invoke();
			}

			private void Awake()
			{
				Subscribe();
			}

			public void Subscribe()
			{
				if (subscribed)
					return;

				WeaponMaterialManager.onWeaponMaterialReload += OnReload;
				subscribed = true;
			}

			private void OnDestroy()
			{
				if (subscribed)
					WeaponMaterialManager.onWeaponMaterialReload -= OnReload;
			}
		}
	}
}
