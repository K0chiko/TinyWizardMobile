using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	public class Cooldown : MonoBehaviour
	{
		private static Cooldown _instance;

		private readonly Dictionary<object, float> _cooldowns = new();

		private void Awake()
		{
			_instance = this;
		}

		private void LateUpdate()
		{
			var toRemove = new HashSet<object>();

			foreach (var entry in _cooldowns)
			{
				if (Time.time > entry.Value)
				{
					toRemove.Add(entry.Key);
				}
			}

			foreach (var item in toRemove)
			{
				_cooldowns.Remove(item);
			}
		}

		public static void Call(object key, float cooldown, Action action)
		{
			if (!_instance._cooldowns.ContainsKey(key))
			{
				action();
				_instance._cooldowns.Add(key, Time.time + cooldown);
			}
		}
	}
}
