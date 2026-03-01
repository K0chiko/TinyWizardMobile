using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Get Travel Time", story: "Get [Time] to [Target] at [Speed] m/s", category: "Action", id: "a1776da5e4b8c92e618d6719175465d5")]
	public partial class GetTravelTimeAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<float> Time;
		[SerializeReference] 
		public BlackboardVariable<Transform> Target;
		[SerializeReference] 
		public BlackboardVariable<float> Speed;

		protected override Status OnStart()
		{
			if (Target.Value == null)
			{
				return Status.Failure;
			}

			float dst = GameObject.transform.position.DistanceTo(Target.Value.position);
			Time.Value = dst / Speed.Value;

			return Status.Success;
		}
	}
}
