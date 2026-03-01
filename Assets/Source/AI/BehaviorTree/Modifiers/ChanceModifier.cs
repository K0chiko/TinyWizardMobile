using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Chance", story: "Execute [Percent] % of the Time", category: "Flow", id: "1556a024f27b3f2adc203f7b4947c650")]
	public partial class ChanceModifier : Modifier
	{
		[SerializeReference]
		public BlackboardVariable<float> Percent;

		protected override Status OnStart()
		{
			if (Child == null)
				return Status.Failure;

			Debug.Assert(Percent.Value >= 0f && Percent.Value <= 100f, "Chance modifier node's 'Percent' parameter must be between 0 and 100!");

			if (UnityEngine.Random.value > (Percent.Value / 100f))
			{
				return Status.Failure;
			}

			return StartNode(Child);
		}

		protected override Status OnUpdate()
		{
			var status = Child.CurrentStatus;

			if (status is Status.Success or Status.Failure)
			{
				EndNode(Child);
			}

			return status;
		}
	}
}
