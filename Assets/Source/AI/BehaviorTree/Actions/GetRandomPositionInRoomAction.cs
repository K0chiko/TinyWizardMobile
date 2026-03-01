using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Get Random Position in Room", story: "Get Random [Position] in Room", category: "Action", id: "045bb9825893d5ac0f3a17bbdff55e5b")]
	public partial class GetRandomPositionInRoomAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;

		[SerializeReference]
		public BlackboardVariable<Vector2> AreaFactor = new(Vector2.one);

		protected override Status OnStart()
		{
			if (Position.Value == null)
				return Status.Failure;

			var room = GameObject.GetComponent<BTAgent>().Room;
			Debug.Assert(room != null);

			var bounds = room.PathfindBounds.bounds;
			bounds.size *= AreaFactor.Value;

			Position.Value = (Vector2)bounds.center + new Vector2()
			{
				x = UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x),
				y = UnityEngine.Random.Range(-bounds.extents.y, bounds.extents.y)
			};

			return Status.Success;
		}
	}
}
