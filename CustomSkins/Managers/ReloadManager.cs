using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomSkins.Managers
{
	public static class ReloadManager
	{
		public delegate void OnReload();
		public static event OnReload onReload;

		public static void ReloadAll() => onReload.Invoke();
	}
}
