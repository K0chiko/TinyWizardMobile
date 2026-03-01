using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Set Speed Factor", story: "Set Speed Factor to [Factor]", category: "Action", id: "c5ef795d0baf0b50b7dabb1a9b8a9a79")]
	public partial class SetSpeedFactorAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<float> Factor;

		protected override Status OnStart()
		{
			if (GameObject.TryGetComponent(out Locomotion movement))
			{
				if (Factor.Value == 1f)
				{
					movement.RemoveSpeedModifier(GameObject);
				}
				else
				{
					movement.ApplySpeedModifier(GameObject, Factor.Value);
				}

				return Status.Success;
			}

			return Status.Failure;
		}
	}
}
