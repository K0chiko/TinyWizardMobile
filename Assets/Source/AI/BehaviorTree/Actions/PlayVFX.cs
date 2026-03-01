using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.VFX;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Play VFX", story: "Play [VFX]", category: "Action", id: "86e6a0e022dd0d9aa3b942f2ccf4ed6e")]
	public partial class PlayVfxAction : Action
	{
    [SerializeReference] public BlackboardVariable<VisualEffect> VFX;
		protected override Status OnStart()
		{
			if (VFX.Value != null)
			{
				VFX.Value.Play();
				return Status.Success;
			}

			return Status.Failure;
		}
	}
}
