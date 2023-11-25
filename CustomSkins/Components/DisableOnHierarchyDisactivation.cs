using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustomSkins.Components
{
	public class DisableOnHierarchyDisactivation : MonoBehaviour
	{
		private void OnDisable()
		{
			gameObject.SetActive(false);
		}
	}
}
