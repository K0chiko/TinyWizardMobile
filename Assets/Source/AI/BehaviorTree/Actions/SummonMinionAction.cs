using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Summon Minion", story: "Summon [Minion] at [Position]", category: "Action", id: "dfc252a0ac00a76ce885a05faed59b38")]
	public partial class SummonMinionAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<GameObject> Minion;
		[SerializeReference] 
		public BlackboardVariable<Vector2> Position;

		protected override Status OnStart()
		{
			if (Minion.Value == null)
				return Status.Failure;

			var prefab = Resources.Load<GameObject>("SummonPortal");
			prefab.Clone(Position.Value);

			var instance = Minion.Value.Clone(Position.Value);

			var toRegister = instance.GetComponent<IAgent>();
			if (toRegister != null && GameObject.TryGetComponent(out IAgent selfAgent))
			{
				selfAgent.Room.RegisterAgent(toRegister);
			}

			return Status.Success;
		}
	}
}
