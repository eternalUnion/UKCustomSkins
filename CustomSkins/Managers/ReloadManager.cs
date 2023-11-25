using CustomSkins.Attributes;
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
		private static List<ReloadMethodDelegate> methods = new List<ReloadMethodDelegate>();

		public delegate void ReloadMethodDelegate();

		internal static void Init()
		{
			if (methods.Count != 0)
				return;

			foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass))
			{
				foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<ReloadMethodAttribute>() != null))
				{
					if (!method.IsStatic)
					{
						Debug.LogError($"Reload method {type.FullName}::{method.Name} is not static");
						continue;
					}

					if (method.ReturnType != typeof(void))
					{
						Debug.LogError($"Reload method {type.FullName}::{method.Name} has a return type");
						continue;
					}

					if (method.GetParameters().Length != 0)
					{
						Debug.LogError($"Reload method {type.FullName}::{method.Name} has parameters");
						continue;
					}

					methods.Add((ReloadMethodDelegate)Delegate.CreateDelegate(typeof(ReloadMethodDelegate), method));
				}
			}
		}
	
		public static void ReloadAll()
		{
			foreach (var method in methods)
			{
				try
				{
					method.Invoke();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}
	}
}
