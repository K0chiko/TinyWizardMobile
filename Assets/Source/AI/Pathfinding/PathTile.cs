using UnityEngine;

namespace Quinn.AI.Pathfinding
{
	public record PathTile
	{
		/// <summary>
		/// Grid pos.
		/// </summary>
		public Vector2Int Position { get; set; }
		/// <summary>
		/// Preceding tile in path.
		/// </summary>
		public PathTile Parent { get; set; }

		/// <summary>
		/// Path length.
		/// </summary>
		public float G { get; set; }
		/// <summary>
		/// Direct distance to destination.
		/// </summary>
		public float H { get; set; }
		/// <summary>
		/// Total cost.
		/// </summary>
		public float F => G + H;
	}
}
