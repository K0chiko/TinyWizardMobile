using UnityEngine;

namespace Quinn
{
	public static class NumberExtensions
	{
		public static float ToDegrees(this float radians)
		{
			return Mathf.Rad2Deg * radians;
		}
	}
}
