using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CustomSkins.Components
{
	public class SkinFieldComponent : MonoBehaviour
	{
		public RawImage icon;
		public Text skinName;

		public Button enableButton;
		public Text enableButtonText;

		public Button upButton;
		public Button downButton;

		public RawImage warningIcon;
		public Text warningText;
	}
}
