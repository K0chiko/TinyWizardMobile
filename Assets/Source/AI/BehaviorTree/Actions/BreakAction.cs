using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Break", story: "Debug Break", category: "Action/Debug", id: "20a7766ed4d95369353f6d1bfe48162f")]
	public partial class BreakAction : Action
	{
		protected override Status OnStart()
		{
			Debug.Break();
			return Status.Success;
		}
	}
}
