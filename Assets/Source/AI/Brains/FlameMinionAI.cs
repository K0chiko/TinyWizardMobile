using FMODUnity;
using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class FlameMinionAI : AIAgent
	{
		[SerializeField, Required]
		private Missile Missile;
		[SerializeField, Required]
		private Transform MissileOrigin;
		[SerializeField]
		private float Interval = 0f;
		[SerializeField]
		private int Count = 1;
		[SerializeField]
		private MissileSpawnBehavior Behavior;
		[SerializeField]
		private float SpreadAngle = 30f;

		[SerializeField, Space]
		private Vector2 IdleDuration = new(1f, 5f);
		[SerializeField]
		private float IdleFireChanceIfNoSpecial = 0.5f;
		[SerializeField]
		private float IdleFireInterval = 1f;
		[SerializeField]
		private float MaxTraverseDistance = 4f;
		[SerializeField]
		private float TraverseFireChance = 0.5f;
		[SerializeField]
		private float TraverseFireInterval = 1f;
		[SerializeField]
		private float IdealMaxDistanceToTarget = 3f;
		[SerializeField]
		private EventReference FireSound;

		[Space, SerializeField]
		private float SpecialChanceOnIdle = 0.2f;
		[SerializeField]
		private int SpecialCount = 8;
		[SerializeField]
		private float SpecialSpread = 360f;
		[SerializeField]
		private MissileSpawnBehavior SpecialBehavior = MissileSpawnBehavior.SpreadEven;
		[SerializeField]
		private EventReference SpecialSound;

		private float _idleEndTime;
		private bool _doesFireOnIdle;
		private float _nextIdleFireTime;

		private Vector2 _traversalOrigin;
		private Vector2 _traversalTarget;
		private bool _doesShootOnTraverse;
		private float _nextTraverseShootTime;

		protected override void OnThink()
		{
			if (!IsRoomStarted)
				return;

			if (ActiveState is null)
			{
				if (Random.value < 0.5f)
				{
					TransitionTo(OnIdle);
				}
				else
				{
					TransitionTo(OnTraverse);
				}
			}

			FaceTarget();
			Animator.SetBool("IsMoving", ActiveState == OnTraverse);
		}

		protected override void OnDamaged(float amount, Vector2 dir, GameObject source)
		{
			base.OnDamaged(amount, dir, source);

			if (ActiveState is null || ActiveState == OnIdle)
			{
				_doesShootOnTraverse = true;
				TransitionTo(OnTraverse);
			}
		}

		protected override void OnDeath()
		{
			DeathTokenSource.Cancel();
			AIManager.Instance.RemoveAgent(this);

			Animator.SetTrigger("Die");
		}

		private bool OnIdle(bool isStart)
		{
			if (isStart)
			{
				_idleEndTime = Time.time + IdleDuration.GetRandom();

				_doesFireOnIdle = false;

				if (Random.value < SpecialChanceOnIdle)
				{
					FireSpecial();
				}
				else
				{
					_doesFireOnIdle = Random.value < IdleFireChanceIfNoSpecial;
				}
			}

			if (_doesFireOnIdle && Time.time > _nextIdleFireTime)
			{
				_nextIdleFireTime = Time.time + IdleFireInterval;
				FireMissile();
			}

			return Time.time > _idleEndTime;
		}

		private bool OnTraverse(bool isStart)
		{
			if (isStart)
			{
				_traversalOrigin = Position;
				_traversalTarget = GetRandomPositionInRadiusInRoom();

				for (int i = 0; i < 100; i++)
				{
					Vector2 target = GetRandomPositionRectInRoom();
					
					if (target.DistanceTo(TargetPos) < IdealMaxDistanceToTarget)
					{
						_traversalTarget = target;
						break;
					}
				}

				_doesShootOnTraverse = Random.value < TraverseFireChance;
			}

			if (_doesShootOnTraverse && Time.time > _nextTraverseShootTime)
			{
				_nextTraverseShootTime = Time.time + TraverseFireInterval;
				FireMissile();
			}

			bool reached = Movement.MoveTo(_traversalTarget);
			bool maxDst = Position.DistanceTo(_traversalOrigin) > MaxTraverseDistance;

			return reached || maxDst;
		}

		private void FireMissile()
		{
			MissileManager.Instance.SpawnMissile(gameObject, Missile, MissileOrigin.position, DirToTarget,
				Count, Interval, Behavior, SpreadAngle);

			Audio.Play(FireSound);
		}

		private void FireSpecial()
		{
			MissileManager.Instance.SpawnMissile(gameObject, Missile, MissileOrigin.position, DirToTarget,
				SpecialCount, SpecialBehavior, SpecialSpread);

			Audio.Play(SpecialSound);
		}
	}
}
