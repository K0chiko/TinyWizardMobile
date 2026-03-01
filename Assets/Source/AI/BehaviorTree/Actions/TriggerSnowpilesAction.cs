using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Trigger Snowpiles", story: "Trigger Snowpiles [Piles]", category: "Action", id: "beed8f1eaf1cffbc4b95807e1f2c043f")]
	public partial class TriggerSnowpilesAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<List<GameObject>> Piles;

		protected override Status OnStart()
		{
			if (Piles.Value.Count == 0)
			{
				return Status.Failure;
			}

			foreach (var pile in Piles.Value)
			{
				pile.GetComponentInChildren<Snowpile>().Spawn();
			}

			return Status.Success;
		}
	}
}
