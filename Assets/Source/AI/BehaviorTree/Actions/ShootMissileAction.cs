using Quinn.MissileSystem;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Shoot Missile", story: "Shoot [Missile] at [Target] from [Origin]", category: "Action", id: "96274c2e72bbc9d80946a5ae6155ee1f")]
	public partial class ShootMissileAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Missile> Missile;
		[SerializeReference]
		public BlackboardVariable<Vector2> Target;
		[SerializeReference]
		public BlackboardVariable<Transform> Origin;

		[SerializeReference]
		public BlackboardVariable<int> Count = new(1);
		[SerializeReference]
		public BlackboardVariable<float> Interval = new(0f);
		[SerializeReference]
		public BlackboardVariable<MissileSpawnBehavior> Behavior = new(MissileSpawnBehavior.Direct);
		[SerializeReference]
		public BlackboardVariable<float> Spread = new(30f);

		[SerializeReference]
		public BlackboardVariable<float> KnockbackSpeed = new(0f);

		[SerializeReference]
		public BlackboardVariable<float> AngleOffset = new(0f);

		private float _endTime;

		protected override Status OnStart()
		{
			if (Missile.Value == null || Origin.Value == null)
				throw new Exception();

			Vector2 dir = Origin.Value.position.DirectionTo(Target.Value);

			if (KnockbackSpeed.Value > 0f && GameObject.TryGetComponent(out Locomotion movement))
			{
				movement.Knockback(-dir, KnockbackSpeed.Value);
			}

			MissileManager.AngleOffset = AngleOffset.Value;

			if (Interval.Value > 0f)
			{
				MissileManager.Instance.SpawnMissile(GameObject, Missile.Value, Origin.Value.position, dir, Count.Value, Interval.Value, Behavior.Value, Spread.Value);
				_endTime = Time.time + ((Count.Value - 1) * Interval.Value);

				return Status.Running;
			}
			else
			{
				MissileManager.Instance.SpawnMissile(GameObject, Missile.Value, Origin.Value.position, dir, Count.Value, Behavior.Value, Spread.Value);
				return Status.Success;
			}
		}

		protected override Status OnUpdate()
		{
			return Time.time > _endTime ? Status.Success : Status.Running;
		}
	}
}
