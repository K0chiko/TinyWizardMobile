using UnityEngine;

namespace Quinn
{
	public class WorldGrid : MonoBehaviour
	{
		[SerializeField]
		private float TileSize = 1f;
		[SerializeField]
		private Vector2 Origin = Vector2.zero;

		public static WorldGrid Instance { get; private set; }

		public void Awake()
		{
			Debug.Assert(Instance == null, "There should only be one WorldGrid instance!");
			Instance = this;
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		public Vector2 GridToWorld(Vector2Int gridPos)
		{
			Vector2 world = (Vector2)gridPos;
			world *= TileSize;
			world += Origin;

			return world;
		}
		public Vector2 GridToWorld(int x, int y)
		{
			return GridToWorld(new(x, y));
		}

		public Vector2Int WorldToGrid(Vector2 worldPos)
		{
			worldPos += Origin;
			var unrounded = (worldPos / TileSize);
			var gridPos = new Vector2Int(Mathf.RoundToInt(unrounded.x), Mathf.RoundToInt(unrounded.y));

			return gridPos;
		}
		public Vector2Int WorldToGrid(float x, float y)
		{
			return WorldToGrid(new(x, y));
		}
	}
}
