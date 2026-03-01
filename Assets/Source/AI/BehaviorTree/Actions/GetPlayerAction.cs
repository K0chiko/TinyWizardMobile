using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Quinn.PlayerSystem;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Get Player", story: "Get Player as [Target]", category: "Action", id: "f6b1636897ba4f79cf22c67651dca7d1")]
	public partial class GetPlayerAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Health> Target;

		protected override Status OnStart()
		{
			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (PlayerManager.Instance == null)
				return Status.Running;

			var player = PlayerManager.Instance.Player;

			if (player == null)
				return Status.Running;

			Target.Value = player.GetComponent<Health>();
			return Status.Success;
		}
	}
}
