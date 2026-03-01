using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Get Target Position In Future", story: "Get Position [Position] at [Target] After [Time] Seconds", category: "Action", id: "448938f66916be4f5b30fd336f6a1dc9")]
	public partial class GetTargetPositionInFutureAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Transform> Target;
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;
		[SerializeReference]
		public BlackboardVariable<float> Time;

		protected override Status OnStart()
		{
			if (Target.Value == null)
			{
				return Status.Failure;
			}

			if (!Target.Value.gameObject.TryGetComponent(out Rigidbody2D rb))
			{
				return Status.Failure;
			}

			Vector3 center = Target.Value.position;
			if (Target.Value.TryGetComponent(out Collider2D collider))
			{
				center = collider.bounds.center;
			}

			Vector3 vel = rb.linearVelocity;
			Position.Value = center + (vel * Time.Value);

			return Status.Success;
		}
	}
}
