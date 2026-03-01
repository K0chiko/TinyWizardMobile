using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	public class Draw : MonoBehaviour
	{
		class RectCmd
		{
			public Vector2 Center;
			public Vector2 Size;
			public Color Color;
			public float EndTime;
		}

		private static Draw _instance;

		private readonly HashSet<RectCmd> _rects = new();

		private void Awake()
		{
			if (_instance == null )
			{
				Destroy(_instance);
			}

			_instance = this;
		}

		private void OnDestroy()
		{
			if (_instance == this)
				_instance = null;
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				var toRemove = new HashSet<RectCmd>();

				foreach (var rect in _rects)
				{
					Gizmos.color = rect.Color;
					Gizmos.DrawWireCube(rect.Center, rect.Size);

					if (Time.time > rect.EndTime)
					{
						toRemove.Add(rect);
					}
				}

				foreach (var rect in toRemove)
				{
					_rects.Remove(rect);
				}
			}
		}

		public static void Rect(Vector2 center, Vector2 size, Color color, float duration = 0f)
		{
			_instance._rects.Add(new RectCmd()
			{
				Center = center,
				Size = size,
				Color = color,
				EndTime = Time.time + duration
			});
		}

		public static void Line(Vector2 start, Vector2 end, Color color, float duration = 0f)
		{
			Debug.DrawLine(start, end, color, duration);
		}
	}
}
