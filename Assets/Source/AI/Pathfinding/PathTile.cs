using UnityEngine;

namespace Quinn.AI.Pathfinding
{
	public record PathTile
	{
		public Vector2Int Position { get; set; }
		public PathTile Parent { get; set; }

		public float G { get; set; }
		public float H { get; set; }
		public float F => G + H;
	}
}
