using UnityEngine;

namespace Quinn
{
	public static class TransformExtensions
	{
		public static void DestroyChildren(this Transform transform)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Object.Destroy(transform.GetChild(i).gameObject);
			}
		}

		public static Transform[] GetChildren(this Transform transform)
		{
			var children = new Transform[transform.childCount];

			for (int i = 0; i < transform.childCount; i++)
			{
				children[i] = transform.GetChild(i);
			}

			return children;
		}
	}
}
