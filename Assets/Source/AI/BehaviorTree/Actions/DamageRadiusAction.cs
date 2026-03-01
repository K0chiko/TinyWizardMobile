using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Damage Radius", story: "Apply Damage [D] at Position [P] in Radius [R] m", category: "Action", id: "2483360e8b22b15541b6a91ed1cb8286")]
	public partial class DamageRadiusAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> P;
		[SerializeReference]
		public BlackboardVariable<float> D;
		[SerializeReference]
		public BlackboardVariable<float> R;

		[SerializeReference]
		public BlackboardVariable<Team> TargetTeam = new(Quinn.Team.Player);

		protected override Status OnStart()
		{
			foreach (var collider in Physics2D.OverlapCircleAll(P.Value, R.Value))
			{
				if (collider.TryGetComponent(out IDamageable dmg))
				{
					if (dmg.Team == TargetTeam.Value)
					{
						dmg.TakeDamage(D.Value, P.Value.DirectionTo(collider.bounds.center), Team.Monster, GameObject);
					}
				}
			}

			return Status.Success;
		}
	}
}
