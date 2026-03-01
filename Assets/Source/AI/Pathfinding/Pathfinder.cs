using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Quinn.AI.Pathfinding
{
	public class Pathfinder : MonoBehaviour
	{
		enum DebugLogLevel
		{
			None = 0, Minimal = 1, Verbose = 2
		}

		[SerializeField]
		private int ProcessChunkSize = 200;
		[SerializeField]
		private DebugLogLevel LogLevel;

		public static Pathfinder Instance { get; private set; }

		public Tilemap Navmesh { get; private set; }

		private void Awake()
		{
			Debug.Assert(Instance == null, "There should only be one instance of Pathfinder!");
			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public async Awaitable<Vector2Int[]> FindPath(Vector2Int origin, Vector2Int destination, CancellationToken token = default)
		{
			if (Navmesh == null)
				return System.Array.Empty<Vector2Int>();

			var open = new Dictionary<Vector2Int, (float g, float h, Vector2Int parent)>();
			var closed = new Dictionary<Vector2Int, (float g, float h, Vector2Int parent)>();

			Vector2Int current = origin;
			open.Add(current, (0f, origin.DistanceTo(destination), origin));

			int processCount = 0;

			// While there are open tiles to explore and we haven't yet found the destination.
			while (open.Count > 0)
			{
				processCount++;
				if (processCount > ProcessChunkSize)
				{
					processCount = 0;
					await Awaitable.NextFrameAsync(token);
				}

				if (token.IsCancellationRequested)
					return System.Array.Empty<Vector2Int>();

				//await Awaitable.WaitForSecondsAsync(0.02f);

				Vector2Int bestTile = current;
				float bestFCost = float.PositiveInfinity;

				// Get lowest f-cost from open.
				foreach (var tile in open)
				{
					float fCost = tile.Value.g + tile.Value.h;
					if (fCost < bestFCost)
					{
						bestTile = tile.Key;
						bestFCost = fCost;
					}
				}

				current = bestTile;

				// Close and explore around current.
				closed.Add(current, open[current]);
				float currentG = open[current].g;
				open.Remove(current);

				if (LogLevel is DebugLogLevel.Verbose)
					Draw.Rect(WorldGrid.Instance.GridToWorld(current), Vector2.one, new Color(1f, 0.25f, 0f), float.PositiveInfinity);

				// Explore neighbors.
				for (int x = -1; x < 2; x++)
				{
					for (int y = -1; y < 2; y++)
					{
						var tile = new Vector2Int(x, y);
						tile += current;

						if (tile == current || closed.ContainsKey(tile) || IsObstacle(tile))
							continue;

						// No diagonal pathing.
						//if (x != 0 && y != 0)
							//continue;

						float g = currentG + current.DistanceTo(tile);
						float h = tile.DistanceTo(destination);
						float f = g + h;

						if (open.TryGetValue(tile, out var existing))
						{
							if (existing.g + existing.h < f)
							{
								// Do not update tiles that have cheaper f-costs.
								continue;
							}
							else
							{
								open.Remove(tile);
							}
						}

						open.Add(tile, (g, h, current));

						if (LogLevel is DebugLogLevel.Verbose)
						{
							Draw.Rect(WorldGrid.Instance.GridToWorld(tile), Vector2.one, Color.cyan, float.PositiveInfinity);
							Draw.Line(WorldGrid.Instance.GridToWorld(current), WorldGrid.Instance.GridToWorld(tile), Color.red, float.PositiveInfinity);
						}
					}
				}

				// If at destination, return path.
				if (current == destination || open.Count == 0)
				{
					var path = new List<Vector2Int>();
					Vector2Int tile = destination;

					if (open.Count == 0)
					{
						if (LogLevel >= DebugLogLevel.Minimal)
						{
							Debug.Log("Returning incomplete path.");
						}

						tile = closed.GetClosestTo(x => x.Value.h).Key;
					}

					// Backtrace path.
					while (tile != origin)
					{
						path.Insert(0, tile);
						tile = closed[tile].parent;
					}

					path = Simplify(path);

					// Draw path lines.
					if (LogLevel >= DebugLogLevel.Minimal)
					{
						Draw.Rect(destination, Vector2.one, Color.yellow, float.PositiveInfinity);
						for (int i = 0; i < path.Count - 1; i++)
						{
							var a = WorldGrid.Instance.GridToWorld(path[i]);
							var b = WorldGrid.Instance.GridToWorld(path[i + 1]);

							Draw.Rect(a, Vector2.one, Color.black, float.PositiveInfinity);
							Draw.Line(Vector2.Lerp(a, b, 0.1f), Vector2.Lerp(a, b, 0.9f), Color.white, float.PositiveInfinity);

							//await Awaitable.WaitForSecondsAsync(0.003f);
						}

						Draw.Rect(destination, Vector2.one, Color.yellow, float.PositiveInfinity);
					}

					return path.ToArray();
				}
			}

			Debug.LogWarning("Failed to find path!");
			return System.Array.Empty<Vector2Int>();
		}
		public async Awaitable<Vector2[]> FindPath(Vector2 origin, Vector2 destination, CancellationToken token = default)
		{
			Vector2Int a = WorldGrid.Instance.WorldToGrid(origin);
			Vector2Int b = WorldGrid.Instance.WorldToGrid(destination);

			Vector2Int[] indices = await FindPath(a, b, token);
			var path = new List<Vector2>();

			foreach (var index in indices)
			{
				path.Add(WorldGrid.Instance.GridToWorld(index));
			}

			return path.ToArray();
		}

		public bool IsTraversable(Vector2Int gridPos)
		{
			return Navmesh.HasTile((Vector3Int)gridPos);
		}
		public bool IsObstacle(Vector2Int gridPos)
		{
			return !Navmesh.HasTile((Vector3Int)gridPos);
		}

		public float GetDistance(Vector2Int a, Vector2Int b)
		{
			return ((Vector2)a).DistanceTo((Vector2)b);
		}

		public void SetNavmesh(Tilemap navmesh)
		{
			Navmesh = navmesh;
		}

		private List<Vector2Int> Simplify(List<Vector2Int> points)
		{
			if (points.Count <= 2)
			{
				return new List<Vector2Int>(points);
			}

			static Vector2Int Normalized(Vector2Int v)
			{
				return new Vector2Int(Mathf.Clamp(v.x, -1, 1), Mathf.Clamp(v.y, -1, 1));
			}

			var list = new List<Vector2Int>()
			{
				points[0]
			};

			for (int i = 1; i < points.Count - 1; i++)
			{
				Vector2Int previous = points[i - 1];
				Vector2Int current = points[i];
				Vector2Int next = points[i + 1];

				Vector2Int a = Normalized(current - previous);
				Vector2Int b = Normalized(next - current);

				if (a != b)
				{
					list.Add(current);
				}
			}

			list.Add(points[^1]);
			return list;
		}
	}
}
