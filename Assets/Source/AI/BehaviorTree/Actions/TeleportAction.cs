using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Teleport", story: "Teleport to [Position]", category: "Action", id: "262a7e506e8416c755ff8c635ffeaf97")]
	public partial class TeleportAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;

		protected override Status OnStart()
		{
			GameObject.transform.position = Position.Value;
			return Status.Success;
		}
	}
}
