using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Trigger Indirect Caller", story: "Trigger Indirect Caller [Caller]", category: "Action", id: "1889858ec8ba1e93d9a3f1ae3525e23d")]
	public partial class TriggerIndirectCallerAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<IndirectCaller> Caller;

		protected override Status OnStart()
		{
			if (Caller.Value != null)
			{
				Caller.Value.Call();
				return Status.Success;
			}

			return Status.Failure;
		}
	}
}
