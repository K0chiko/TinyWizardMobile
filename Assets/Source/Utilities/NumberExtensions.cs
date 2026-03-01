using UnityEngine;

namespace Quinn
{
	public static class NumberExtensions
	{
		public static float ToRadians(this float degrees)
		{
			return Mathf.Deg2Rad * degrees;
		}

		public static float ToDegrees(this float radians)
		{
			return Mathf.Rad2Deg * radians;
		}
		
		public static Vector2 ToDirection(this float degrees)
		{
			return new(Mathf.Cos(degrees), Mathf.Sin(degrees));
		}
	}
}
