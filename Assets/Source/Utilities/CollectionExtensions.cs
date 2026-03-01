using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quinn
{
	public static class CollectionExtensions
	{
		public static T GetRandom<T>(this IEnumerable<T> collection)
		{
			if (!collection.Any())
				return default;

			return collection.ElementAt(Random.Range(0, collection.Count()));
		}

		public static T GetWeightedRandom<T>(this IEnumerable<T> collection, System.Func<T, float> getWeightCallback)
		{
			if (!collection.Any())
			{
				return default;
			}

			if (collection.Count() == 1)
			{
				return collection.First();
			}

			float sum = collection.Sum(x => getWeightCallback(x));

			foreach (var item in collection)
			{
				if (Random.value < getWeightCallback(item) / sum)
				{
					return item;
				}
			}

			return collection.GetRandom();
		}

		public static T GetClosestTo<T>(this IEnumerable<T> collection, Vector2 point) where T : Component
		{
			T t = default;
			float nearest = float.PositiveInfinity;

			foreach (var item in collection)
			{
				float dst = item.transform.position.DistanceTo(point);

				if (dst < nearest)
				{
					nearest = dst;
					t = item;
				}
			}

			return t;
		}
		public static T GetClosestTo<T>(this IEnumerable<T> collection, System.Func<T, float> dstCallback)
		{
			T t = default;
			float nearest = float.PositiveInfinity;

			foreach (var item in collection)
			{
				float dst = dstCallback(item);

				if (dst < nearest)
				{
					nearest = dst;
					t = item;
				}
			}

			return t;
		}

		public static void ForEach<T>(this IEnumerable<T> collection, System.Action<T> action)
		{
			foreach (var item in collection)
			{
				action(item);
			}
		}
	}
}
