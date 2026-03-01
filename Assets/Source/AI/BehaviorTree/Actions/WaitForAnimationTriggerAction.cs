using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Wait For Animation Trigger", story: "Wait Until Trigger [Key] is Set", category: "Action", id: "4a4f8a242c977f05e1ab7ffa375e1827")]
	public partial class WaitForAnimationTriggerAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<string> Key;

		private BTAgent _agent;

		protected override Status OnStart()
		{
			if (!GameObject.TryGetComponent(out _agent))
			{
				return Status.Failure;
			}

			if (_agent.GetAnimationTrigger(Key.Value))
			{
				return Status.Success;
			}

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (_agent.GetAnimationTrigger(Key.Value))
			{
				return Status.Success;
			}

			return Status.Running;
		}
	}
}
