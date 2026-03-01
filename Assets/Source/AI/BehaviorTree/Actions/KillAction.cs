using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Kill", story: "Kill [Target]", category: "Action", id: "2c5cc53b518ff36f979f106900c96539")]
	public partial class KillAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Health> Target;

		protected override Status OnStart()
		{
			if (Target.Value != null)
			{
				Target.Value.Kill();
				return Status.Success;
			}

			return Status.Failure;
		}
	}
}
