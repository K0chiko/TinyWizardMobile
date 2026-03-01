using System.Linq;
using UnityEngine;

namespace Quinn
{
	public static class GameObjectExtensions
	{
		public static GameObject Clone(this GameObject prefab, Vector2 position = default)
		{
			return Object.Instantiate(prefab, position, Quaternion.identity);
		}
		public static GameObject Clone(this GameObject prefab, Transform parent)
		{
			return Object.Instantiate(prefab, parent);
		}
		public static GameObject Clone(this GameObject prefab, Vector2 position, Quaternion rotation, Transform parent)
		{
			return Object.Instantiate(prefab, position, rotation, parent);
		}

		public static async Awaitable<GameObject> CloneAsync(this GameObject prefab, Vector2 position = default)
		{
			var clones = await Object.InstantiateAsync(prefab, position, Quaternion.identity);
			if (clones == null) return null;
			return clones.FirstOrDefault();
		}
		public static async Awaitable<GameObject> CloneAsync(this GameObject prefab, Vector2 position, Quaternion rotation, Transform parent)
		{
			var clones = await Object.InstantiateAsync(prefab, parent, position, rotation);
			if (clones == null) return null;
			return clones.FirstOrDefault();
		}

		public static void Destroy(this GameObject prefab)
		{
			Object.Destroy(prefab);
		}
		public static void Destroy(this GameObject prefab, float delay)
		{
			Object.Destroy(prefab, delay);
		}
	}
}
