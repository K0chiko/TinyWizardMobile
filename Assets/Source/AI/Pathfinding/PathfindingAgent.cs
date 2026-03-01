using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.AI.Pathfinding
{
	public class PathfindingAgent : MonoBehaviour
	{
		[SerializeField, Required]
		private Transform Target;
		[SerializeField]
		private float MoveSpeed = 2f;

		private readonly List<Vector2Int> _path = new();

		private async void Start()
		{
			var path = await Pathfinder.Instance.FindPath(WorldGrid.Instance.WorldToGrid(transform.position), new(5, 5));
			_path.AddRange(path);
		}

		private void Update()
		{
			if (_path.Count > 0)
			{
				Vector2 target = WorldGrid.Instance.GridToWorld(_path[0]);

				if (Vector2.Distance(transform.position, target) > 0.05f)
				{
					transform.Translate(MoveSpeed * Time.deltaTime * ((Vector3)target - transform.position).normalized, Space.World);
				}
				else
				{
					_path.RemoveAt(0);
				}
			}
		}
	}
}
