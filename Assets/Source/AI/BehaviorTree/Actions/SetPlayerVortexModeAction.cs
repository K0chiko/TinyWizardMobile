using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Quinn.PlayerSystem;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Set Player Vortex Mode", story: "Set Player Vortex Mode to [Boolean]", category: "Action", id: "0bf80f3169ec23ba05581511b6ecf5be")]
	public partial class SetPlayerVortexModeAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<bool> Boolean;

		protected override Status OnStart()
		{
			if (Boolean.Value)
				PlayerManager.Instance.Movement.SetVortexOrigin(GameObject.transform);
			else
				PlayerManager.Instance.Movement.ClearVortexOrigin();

			return Status.Success;
		}
	}
}
