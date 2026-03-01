using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Face Position", story: "Face Towards [Position]", category: "Action", id: "e40b59abeb2dbb253b323d6a7de38ed2")]
	public partial class FacePositionAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;

		protected override Status OnStart()
		{
			if (Position.Value == null)
				return Status.Failure;

			Vector2 dir = GameObject.transform.position.DirectionTo(Position.Value);

			Vector3 scale = GameObject.transform.localScale;
			scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);

			GameObject.transform.localScale = scale;
			return Status.Success;
		}
	}
}
