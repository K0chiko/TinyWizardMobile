using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Face Transform", story: "Face [Transform]", category: "Action", id: "56a94d935eda361a23e13a61141923e2")]
	public partial class FaceTransformAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Transform> Transform;

		protected override Status OnStart()
		{
			if (Transform.Value == null)
				return Status.Failure;

			Vector2 dir = GameObject.transform.position.DirectionTo(Transform.Value.position);

			Vector3 scale = GameObject.transform.localScale;
			scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);

			GameObject.transform.localScale = scale;
			return Status.Success;
		}
	}
}
