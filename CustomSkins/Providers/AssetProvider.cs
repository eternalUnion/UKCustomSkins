﻿using CustomSkins.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System.Linq;

namespace CustomSkins.Providers
{
	// Created from a file source, checks all the data files and creates assets (materials, audio clips etc.)
	// File source is closed after the construction
	public class AssetProvider
	{
		private FileProvider _fileProvider;
		private AssetBundle _bundle;

		private List<Tuple<MaterialDefinition, Material>> _materials = new List<Tuple<MaterialDefinition, Material>>();
		private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

		private bool _disposed = false;
		private List<UnityEngine.Object> _unmanagedResources = new List<UnityEngine.Object>();
		internal void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			if (_bundle != null)
			{
				try
				{
					_bundle.Unload(false);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			_bundle = null;

			foreach (var res in _unmanagedResources)
				if (res != null)
					UnityEngine.Object.Destroy(res);

			_unmanagedResources.Clear();

			_fileProvider.Dispose();
		}

		public AssetProvider(FileProvider provider)
		{
			_fileProvider = provider;

			try
			{
				provider.Open();

				if (provider.FileExists("assetbundle"))
				{
					try
					{
						_bundle = AssetBundle.LoadFromMemory(provider.ReadBytes("assetbundle"));
					}
					catch (Exception e)
					{
						Debug.LogError("Caught error while reading asset bundle from skin");
						Debug.LogException(e);
					}
				}

				string materialDataPath = "Data/Materials";
				if (provider.DirectoryExists(materialDataPath))
				{
					foreach (string materialData in provider.GetFiles(materialDataPath))
					{
						try
						{
							MaterialDefinition def = JsonConvert.DeserializeObject<MaterialDefinition>(provider.ReadFile(materialData));
							var mat = CreateMaterial(def);

							_materials.Add(new Tuple<MaterialDefinition, Material>(def, mat));
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}
					}
				}
			}
			finally
			{
				provider.Close();
			}
		}

		// Asset creators
		private static Shader defaultShader = null;
		private Material CreateMaterial(MaterialDefinition matDef)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(AssetProvider));

			Material mat = null;

			// Instantiate reference material
			if (string.IsNullOrEmpty(matDef.baseMaterial))
			{
				if (defaultShader == null)
					defaultShader = Addressables.LoadAssetAsync<Shader>("Assets/Shaders/Main/ULTRAKILL-vertexlit.shader").WaitForCompletion();
				mat = new Material(defaultShader);
			}
			else
			{
				switch (matDef.baseMaterialSource)
				{
					default:
					case ResourceSource.unknown:
						Debug.LogError($"Invalid base material source for {matDef.targetMaterial}");
						return null;

					case ResourceSource.local:
						Debug.LogError($"Base material source for {matDef.targetMaterial} cannot be local");
						return null;

					case ResourceSource.addressables:
						Material baseAddressableMat = Addressables.LoadAssetAsync<Material>(matDef.baseMaterial).WaitForCompletion();
						if (baseAddressableMat == null)
						{
							Debug.LogError($"Base material {matDef.baseMaterial} could not be located in addressable assets");
							return null;
						}

						mat = UnityEngine.Object.Instantiate(baseAddressableMat);
						break;

					case ResourceSource.assetbundle:
						if (_bundle == null)
						{
							Debug.LogError($"Base material {matDef.baseMaterial} could not be loaded from asset bundle because none was loaded");
							return null;
						}

						Material assetbundleBaseMat = _bundle.LoadAsset<Material>(matDef.baseMaterial);
						if (assetbundleBaseMat == null)
						{
							Debug.LogError($"Base material {matDef.baseMaterial} could not be loaded from asset bundle because it was not found");
							return null;
						}

						mat = UnityEngine.Object.Instantiate(assetbundleBaseMat);
						break;
				}
			}

			// Load shader, skip if not defined
			if (!string.IsNullOrEmpty(matDef.shader))
			{
				switch (matDef.shaderSource)
				{
					default:
					case ResourceSource.unknown:
						Debug.LogError($"Invalid shader source for {matDef.targetMaterial}");
						return null;

					case ResourceSource.local:
						Debug.LogError($"Shader source for {matDef.targetMaterial} cannot be local");
						return null;

					case ResourceSource.addressables:
						Shader addressableShader = Addressables.LoadAssetAsync<Shader>(matDef.shader).WaitForCompletion();
						if (addressableShader == null)
						{
							Debug.LogError($"Shader for {matDef.shader} could not be located in addressable assets");
							UnityEngine.Object.Destroy(mat);
							return null;
						}

						mat.shader = addressableShader;
						break;

					case ResourceSource.assetbundle:
						if (_bundle == null)
						{
							Debug.LogError($"Shader {matDef.shader} could not be loaded from asset bundle because none was loaded");
							UnityEngine.Object.Destroy(mat);
							return null;
						}

						Shader assetbundleShader = _bundle.LoadAsset<Shader>(matDef.shader);
						if (assetbundleShader == null)
						{
							Debug.LogError($"Shader {matDef.shader} could not be loaded from asset bundle because it was not found");
							UnityEngine.Object.Destroy(mat);
							return null;
						}

						mat.shader = assetbundleShader;
						break;
				}
			}

			// Load main texture
			if (!string.IsNullOrEmpty(matDef.mainTexture))
			{
				string mainTextureLocation = matDef.mainTexture;

				switch (matDef.mainTextureSource)
				{
					case ResourceSource.unknown:
						Debug.LogWarning($"Unknown value source for {matDef.targetMaterial}. Skipped.");
						break;

					case ResourceSource.local:
						if (_fileProvider.FileExists(mainTextureLocation))
						{
							if (_textures.TryGetValue(mainTextureLocation, out Texture2D cachedTexture))
							{
								mat.mainTexture = cachedTexture;
							}
							else
							{
								try
								{
									Texture2D texture = new Texture2D(2, 2);
									texture.LoadImage(_fileProvider.ReadBytes(mainTextureLocation));
									texture.filterMode = FilterMode.Point;

									mat.mainTexture = texture;
									_unmanagedResources.Add(texture);
									_textures[mainTextureLocation] = texture;
								}
								catch (Exception e)
								{
									Debug.LogException(e);
								}
							}
						}
						break;

					case ResourceSource.addressables:
						Texture2D addressableTexture = Addressables.LoadAssetAsync<Texture2D>(mainTextureLocation).WaitForCompletion();
						if (addressableTexture == null)
						{
							Debug.LogWarning($"Failed to locate texture {matDef.targetMaterial} in addressables. Skipped.");
						}
						else
						{
							mat.mainTexture = addressableTexture;
						}
						break;

					case ResourceSource.assetbundle:
						if (_bundle == null)
						{
							Debug.LogError($"Texture {matDef.targetMaterial} could not be loaded from asset bundle because none was loaded");
							break;
						}

						Texture2D assetbundleTexture = _bundle.LoadAsset<Texture2D>(mainTextureLocation);
						if (assetbundleTexture == null)
						{
							Debug.LogError($"Texture {matDef.targetMaterial} could not be loaded from asset bundle because it was not found");
						}
						else
						{
							mat.mainTexture = assetbundleTexture;
						}
						break;
				}
			}

			// Set offset
			if (!string.IsNullOrEmpty(matDef.mainTextureOffset))
			{
				if (ShaderProperty.TryDeserializeVector(matDef.mainTextureOffset, out Vector4 offset))
					mat.mainTextureOffset = offset;
				else
					Debug.LogWarning($"Could not deserialize texture offset for {matDef.targetMaterial}. Skipping.");
			}

			// Set scale
			if (!string.IsNullOrEmpty(matDef.mainTextureScale))
			{
				if (ShaderProperty.TryDeserializeVector(matDef.mainTextureScale, out Vector4 scale))
					mat.mainTextureScale = scale;
				else
					Debug.LogWarning($"Could not deserialize texture scale for {matDef.targetMaterial}. Skipping.");
			}

			// Shader properties
			if (matDef.shaderProperties != null)
			{
				foreach (var shaderProp in matDef.shaderProperties)
				{
					string propName = shaderProp.Key;
					string propValue = shaderProp.Value.value;

					if (string.IsNullOrEmpty(propName))
					{
						Debug.LogWarning($"Unknown property name for material {matDef.targetMaterial}. Skipped.");
						continue;
					}

					if (propValue == null)
						propValue = string.Empty;

					switch (shaderProp.Value.type)
					{
						case ShaderPropertyType.unknown:
							Debug.LogWarning($"Unknown value type for shader property {propName}. Skipped.");
							break;

						case ShaderPropertyType.color:
							if (ShaderProperty.TryDeserializeColor(propValue, out Color clr))
								mat.SetColor(propName, clr);
							else
								Debug.LogWarning($"Failed to deserialize color from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.colorArray:
							if (ShaderProperty.TryDeserializeColorArray(propValue, out Color[] clrArr))
								mat.SetColorArray(propName, clrArr);
							else
								Debug.LogWarning($"Failed to deserialize color array from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.@float:
							if (ShaderProperty.TryDeserializeFloat(propValue, out float f))
								mat.SetFloat(propName, f);
							else
								Debug.LogWarning($"Failed to deserialize float from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.floatArray:
							if (ShaderProperty.TryDeserializeFloatArray(propValue, out float[] fArr))
								mat.SetFloatArray(propName, fArr);
							else
								Debug.LogWarning($"Failed to deserialize float array from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.@int:
							if (ShaderProperty.TryDeserializeInt(propValue, out int i))
								mat.SetInt(propName, i);
							else
								Debug.LogWarning($"Failed to deserialize int from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.texture:
							Texture2D textureValue = null;

							switch (shaderProp.Value.valueSource)
							{
								case ResourceSource.unknown:
									Debug.LogWarning($"Unknown value source for {propName}. Skipped.");
									break;

								case ResourceSource.local:
									if (_fileProvider.FileExists(propValue))
									{
										if (!_textures.TryGetValue(propValue, out textureValue))
										{
											try
											{
												textureValue = new Texture2D(2, 2);
												textureValue.LoadImage(_fileProvider.ReadBytes(propValue));
												textureValue.filterMode = FilterMode.Point;
												_textures[propValue] = textureValue;
												_unmanagedResources.Add(textureValue);
											}
											catch (Exception e)
											{
												Debug.LogException(e);
											}
										}
									}
									break;

								case ResourceSource.addressables:
									textureValue = Addressables.LoadAssetAsync<Texture2D>(propValue).WaitForCompletion();
									if (textureValue == null)
									{
										Debug.LogWarning($"Failed to locate texture {propValue} in addressables. Skipped.");
									}
									break;

								case ResourceSource.assetbundle:
									if (_bundle == null)
									{
										Debug.LogError($"Texture {propValue} could not be loaded from asset bundle because none was loaded");
										break;
									}

									textureValue = _bundle.LoadAsset<Texture2D>(propValue);
									if (textureValue == null)
									{
										Debug.LogError($"Texture {propValue} could not be loaded from asset bundle because it was not found");
									}
									break;
							}

							if (textureValue != null)
								mat.SetTexture(propName, textureValue);
							break;

						case ShaderPropertyType.textureOffset:
							if (ShaderProperty.TryDeserializeVector(propValue, out Vector4 off))
								mat.SetTextureOffset(propName, off);
							else
								Debug.LogWarning($"Failed to deserialize texture offset from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.textureScale:
							if (ShaderProperty.TryDeserializeVector(propValue, out Vector4 scl))
								mat.SetTextureScale(propName, scl);
							else
								Debug.LogWarning($"Failed to deserialize texture scale from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.vector:
							if (ShaderProperty.TryDeserializeVector(propValue, out Vector4 vec))
								mat.SetVector(propName, vec);
							else
								Debug.LogWarning($"Failed to deserialize vector from {propValue}. Skipped.");
							break;

						case ShaderPropertyType.vectorArray:
							if (ShaderProperty.TryDeserializeVectorArray(propValue, out Vector4[] vecArr))
								mat.SetVectorArray(propName, vecArr);
							else
								Debug.LogWarning($"Failed to deserialize vector from {propValue}. Skipped.");
							break;
					}
				}
			}

			// Set alpha last as shader property can be used to modify main texture as well
			if (matDef.mainTextureAlpha != null && mat.mainTexture != null && mat.mainTexture is Texture2D mainTex)
			{
				Texture2D textureValue = null;

				switch (matDef.mainTextureAlphaSource)
				{
					case ResourceSource.unknown:
						Debug.LogWarning($"Unknown main texture alpha source for {mat.name}. Skipped.");
						break;

					case ResourceSource.local:
						if (_fileProvider.FileExists(matDef.mainTextureAlpha))
						{
							if (!_textures.TryGetValue(matDef.mainTextureAlpha, out textureValue))
							{
								try
								{
									textureValue = new Texture2D(2, 2);
									textureValue.LoadImage(_fileProvider.ReadBytes(matDef.mainTextureAlpha));
									textureValue.filterMode = FilterMode.Point;
									_textures[matDef.mainTextureAlpha] = textureValue;
									_unmanagedResources.Add(textureValue);
								}
								catch (Exception e)
								{
									Debug.LogException(e);
								}
							}
						}
						break;

					case ResourceSource.addressables:
						textureValue = Addressables.LoadAssetAsync<Texture2D>(matDef.mainTextureAlpha).WaitForCompletion();
						if (textureValue == null)
						{
							Debug.LogWarning($"Failed to locate texture {matDef.mainTextureAlpha} in addressables. Skipped.");
						}
						break;

					case ResourceSource.assetbundle:
						if (_bundle == null)
						{
							Debug.LogError($"Texture {matDef.mainTextureAlpha} could not be loaded from asset bundle because none was loaded");
							break;
						}

						textureValue = _bundle.LoadAsset<Texture2D>(matDef.mainTextureAlpha);
						if (textureValue == null)
						{
							Debug.LogError($"Texture {matDef.mainTextureAlpha} could not be loaded from asset bundle because it was not found");
						}
						break;
				}

				if (textureValue != null)
				{
					if (textureValue.width != mainTex.width || textureValue.height != mainTex.height)
					{
						Texture2D resizedAlpha = new Texture2D(textureValue.width, textureValue.height);
						resizedAlpha.SetPixels(textureValue.GetPixels());
						resizedAlpha.Resize(mainTex.width, mainTex.height);
						resizedAlpha.Apply();

						textureValue = resizedAlpha;
					}

					Color[] newPixels = mainTex.GetPixels();
					Color[] alphaPixels = textureValue.GetPixels();
					int limit = Math.Min(newPixels.Length, alphaPixels.Length);
					for (int pixel = 0; pixel < limit; pixel++)
					{
						newPixels[pixel].a = alphaPixels[pixel].grayscale;
					}

					Texture2D newMainTex = new Texture2D(mainTex.width, mainTex.height);
					newMainTex.SetPixels(newPixels);
					newMainTex.Apply();
					mat.mainTexture = newMainTex;
				}
			}

			_unmanagedResources.Add(mat);
			return mat;
		}
	
		// Asset provider methods
		public bool TryGetMaterial(string materialName, out Material mat, out MaterialDefinition matDef)
		{
			var material = _materials.Where(mat => mat.Item1.targetMaterial == materialName).FirstOrDefault();

			if (material == null)
			{
				mat = null;
				matDef = null;
				return false;
			}

			matDef = material.Item1;
			mat = material.Item2;
			return true;
		}

		public bool TryGetGeneralWeaponMaterial(string materialName, out Material mat, out MaterialDefinition matDef)
		{
			foreach (var material in _materials.Where(mat => mat.Item1.targetMaterial == materialName))
			{
				var matInfo = material.Item1;
				if (matInfo.filters != null)
				{
					if (matInfo.filters.weaponNumber != -1)
						continue;

					if (matInfo.filters.weaponVariation != WeaponVariationFilter.all)
						continue;

					if (matInfo.filters.weaponType != WeaponTypeFilter.all)
						continue;
				}

				matDef = material.Item1;
				mat = material.Item2;

				return true;
			}

			mat = null;
			matDef = null;
			return false;
		}

		public bool TryGetWeaponMaterial(string materialName, int weaponNumber, WeaponVariationFilter weaponVariation, WeaponTypeFilter weaponType, out Material mat, out MaterialDefinition matDef)
		{
			Tuple<MaterialDefinition, Material> bestMaterial = null;
			int bestMaterialMatchCount = -1;

			foreach (var material in _materials.Where(mat => mat.Item1.targetMaterial == materialName))
			{
				int matchCount = 0;

				var matInfo = material.Item1;
				if (matInfo.filters != null)
				{
					if (matInfo.filters.weaponNumber != -1)
					{
						if (matInfo.filters.weaponNumber != weaponNumber)
							continue;

						matchCount += 1;
					}

					if (matInfo.filters.weaponVariation != WeaponVariationFilter.all)
					{
						if (matInfo.filters.weaponVariation != weaponVariation)
							continue;

						matchCount += 1;
					}

					if (matInfo.filters.weaponType != WeaponTypeFilter.all)
					{
						if (matInfo.filters.weaponType != weaponType)
							continue;

						matchCount += 1;
					}
				}

				if (matchCount > bestMaterialMatchCount)
				{
					bestMaterialMatchCount = matchCount;
					bestMaterial = material;

					// Best case
					if (matchCount == 3)
						break;
				}
			}

			if (bestMaterialMatchCount == -1)
			{
				mat = null;
				matDef = null;
				return false;
			}

			matDef = bestMaterial.Item1;
			mat = bestMaterial.Item2;
			return true;
		}
	}

}
