using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CustomSkins.Managers
{
	public abstract class AsyncAsset
	{
		public bool isDone = false;

		public abstract void WaitForCompletion();
	}

	public class AsyncAsset<T> : AsyncAsset where T : UnityEngine.Object
	{
		public T value;
		private AsyncOperationHandle<T> handle;

		public AsyncAsset(string address)
		{
			AddressableAssetManager.awaitedAssets.Add(this);
			handle = Addressables.LoadAssetAsync<T>(address);

			handle.Completed += (val) =>
			{
				isDone = true;
				value = val.Result;
			};
		}

		public override void WaitForCompletion()
		{
			handle.WaitForCompletion();
			isDone = true;
			value = handle.Result;
		}
	}

	public static class AddressableAssetManager
	{
		public static List<AsyncAsset> awaitedAssets = new List<AsyncAsset>();

		public static void EnsureLoaded()
		{
			foreach (var awaitedAsset in awaitedAssets)
			{
				if (awaitedAsset.isDone)
					continue;

				awaitedAsset.WaitForCompletion();
			}

			awaitedAssets.Clear();
		}

		internal static void Init()
		{
			Materials.Init();
		}

		public static class Materials
		{
			internal static void Init()
			{
				defaultRevolverMaterialHandle = new AsyncAsset<Material>("Assets/Models/Weapons/Revolver/Pistol New.mat");
			}

			private static AsyncAsset<Material> defaultRevolverMaterialHandle;
			public static Material defaultRevolverMaterial
			{
				get
				{
					if (!defaultRevolverMaterialHandle.isDone)
						defaultRevolverMaterialHandle.WaitForCompletion();

					return defaultRevolverMaterialHandle.value;
				}
			}
		}
	}
}
