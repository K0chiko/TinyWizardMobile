using UnityEngine;

namespace Quinn
{
	public static class Vector2Utility
	{
		public static float DistanceTo(this Vector3 origin, Vector3 target)
		{
			return Vector3.Distance(origin, target);
		}
		public static float DistanceTo(this Vector2 origin, Vector2 target)
		{
			return Vector2.Distance(origin, target);
		}
		public static float DistanceTo(this Vector2Int origin, Vector2Int target)
		{
			return (target - origin).magnitude;
		}

		public static Vector3 DirectionTo(this Vector3 origin, Vector3 target)
		{
			return (target - origin).normalized;
		}
		public static Vector2 DirectionTo(this Vector2 origin, Vector2 target)
		{
			return (target - origin).normalized;
		}
		public static Vector2Int DirectionTo(this Vector2Int origin, Vector2Int target)
		{
			return target - origin;
		}

		public static bool IsVertical(this Vector2 v)
		{
			return Mathf.Abs(v.y) > Mathf.Abs(v.x);
		}
		public static bool IsVertical(this Vector2Int v)
		{
			return Mathf.Abs(v.y) > Mathf.Abs(v.x);
		}

		public static bool IsHorizontal(this Vector2 v)
		{
			return Mathf.Abs(v.x) > Mathf.Abs(v.y);
		}
		public static bool IsHorizontal(this Vector2Int v)
		{
			return Mathf.Abs(v.x) > Mathf.Abs(v.y);
		}

		public static bool IsCardinal(this Vector2 v)
		{
			return (v.x != 0f && v.y == 0f) || (v.y != 0f && v.x == 0f);
		}
		public static bool IsCardinal(this Vector2Int v)
		{
			return (Mathf.Abs(v.x) != 0 && v.y == 0) || (Mathf.Abs(v.y) != 0 && v.x == 0);
		}

		public static bool IsOrdinal(this Vector2 v)
		{
			return !IsCardinal(v);
		}
		public static bool IsOrdinal(this Vector2Int v)
		{
			return !IsCardinal(v);
		}

		public static float GetRandom(this Vector2 v)
		{
			return Random.Range(v.x, v.y);
		}
	}
}
