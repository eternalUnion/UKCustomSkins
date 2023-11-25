using CustomSkins.Components;
using Newtonsoft.Json.Linq;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Fields
{
	public class CustomSkinField : CustomConfigField
	{
		private static GameObject asset;

		private SkinFieldComponent currentUi;

		public Action onEnableButton;
		public Action onUpButton;
		public Action onDownButton;

		public override void OnHiddenChange(bool selfHidden, bool hierarchyHidden)
		{
			if (currentUi != null)
				currentUi.gameObject.SetActive(!hierarchyHidden);
		}

		public override void OnInteractableChange(bool selfHidden, bool hierarchyHidden)
		{
			if (currentUi != null)
			{
				currentUi.enableButton.interactable = hierarchyInteractable;
				currentUi.upButton.interactable = hierarchyInteractable;
				currentUi.downButton.interactable = hierarchyInteractable;
			}
		}

		private string displayText
		{
			get => $"{skinName}\n<color=grey>by {(string.IsNullOrEmpty(authorName) ? "<unknown>" : authorName)}</color>";
		}

		private string _skinName = "";
		public string skinName
		{
			get => _skinName;
			set
			{
				_skinName = value;
				if (currentUi != null)
					currentUi.skinName.text = displayText;
			}
		}

		private string _authorName  = "";
		public string authorName
		{
			get => _authorName;
			set
			{
				_authorName = value;
				if (currentUi != null)
					currentUi.skinName.text = displayText;
			}
		}

		private string _enableButtonText = "<color=lime>+</color>";
		public string enableButtonText
		{
			get => _enableButtonText;
			set
			{
				_enableButtonText = value;
				if (currentUi != null)
					currentUi.enableButtonText.text = value;
			}
		}

		private bool _movable = false;
		public bool movable
		{
			get => _movable;
			set
			{
				_movable = value;
				if (currentUi != null)
				{
					currentUi.upButton.gameObject.SetActive(value);
					currentUi.downButton.gameObject.SetActive(value);
				}
			}
		}

		private bool _showWarning = false;
		public bool showWarning
		{
			get => _showWarning;
			set
			{
				_showWarning = value;
				if (currentUi != null)
					currentUi.warningIcon.gameObject.SetActive(value);
			}
		}

		private string _warningText = "Made for a newer version of Custom Skins";
		public string warningText
		{
			get => _warningText;
			set
			{
				_warningText = value;
				if (currentUi != null)
					currentUi.warningText.text = _warningText;
			}
		}

		public byte[] iconHash;

		private Texture2D _icon;
		public Texture2D icon
		{
			get => _icon;
			set
			{
				_icon = value;
				if (currentUi != null)
					currentUi.icon.texture = value;
			}
		}

		public CustomSkinField(ConfigPanel panel) : base(panel, 600, 100)
		{
			if (currentUi != null)
				SetUiValues();
		}

		protected override void OnCreateUI(RectTransform fieldUI)
		{
			fieldUI.sizeDelta = new Vector2(600, 100);

			if (asset == null)
				asset = Plugin.bundle.LoadAsset<GameObject>("assets/custom/customskins/fields/skinfield.prefab");

			currentUi = UnityEngine.Object.Instantiate(asset, fieldUI).GetComponent<SkinFieldComponent>();

			currentUi.enableButton.onClick.AddListener(() => { if (onEnableButton != null) onEnableButton.Invoke(); });
			currentUi.upButton.onClick.AddListener(() => { if (onUpButton != null) onUpButton.Invoke(); });
			currentUi.downButton.onClick.AddListener(() => { if (onDownButton != null) onDownButton.Invoke(); });

			SetUiValues();
		}

		private void SetUiValues()
		{
			if (currentUi == null)
				return;

			currentUi.gameObject.SetActive(!hierarchyHidden);

			currentUi.icon.texture = _icon;

			currentUi.enableButton.interactable = hierarchyInteractable;
			currentUi.upButton.interactable = hierarchyInteractable;
			currentUi.downButton.interactable = hierarchyInteractable;

			currentUi.skinName.text = displayText;
			currentUi.enableButtonText.text = _enableButtonText;

			currentUi.upButton.gameObject.SetActive(_movable);
			currentUi.downButton.gameObject.SetActive(_movable);

			currentUi.warningIcon.gameObject.SetActive(_showWarning);
			currentUi.warningText.text = _warningText;
		}
	}
}
