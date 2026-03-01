using Quinn.DungeonGeneration;
using Quinn.PlayerSystem;
using System.Threading;
using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(AIMovement))]
	public abstract class AIAgent : MonoBehaviour, IAgent
	{
		public Health Health { get; private set; }
		public Team Team => Health.Team;

		public CancellationTokenSource DeathTokenSource { get; private set; } = new();

		protected Animator Animator { get; private set; }
		protected AIMovement Movement { get; private set; }
		protected Vector2 Position => transform.position;

		protected Transform Target { get; private set; }
		protected Health TargetHealth { get; private set; }
		protected Room Room { get; private set; }
		protected bool IsRoomStarted { get; private set; }

		protected Vector2 TargetPos
		{
			get
			{
				if (Target == null)
					return transform.position;

				return Target.position;
			}
		}
		protected float DstToTarget => transform.position.DistanceTo(TargetPos);
		protected Collider2D TargetCollider;
		protected Bounds TargetBounds
		{
			get
			{
				if (TargetCollider == null)
				{
					return new Bounds();
				}

				return TargetCollider.bounds;
			}
		}
		protected Vector2 DirToTarget => transform.position.DirectionTo(TargetBounds.center);

		protected AIState ActiveState { get; private set; }

		Room IAgent.Room => Room;

		private bool _isActiveFirst;

		protected virtual void Awake()
		{
			Animator = GetComponent<Animator>();
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();

			Health.OnDamaged += OnDamaged;
			Health.OnDeath += OnDeath;

			AIManager.Instance.AddAgent(this);
		}

		protected virtual void Update()
		{
			OnThink();

			if (ActiveState != null)
			{
				bool finished = false;

				if (_isActiveFirst)
				{
					finished = ActiveState.Invoke(true);
					_isActiveFirst = false;
				}
				else
				{
					finished = ActiveState.Invoke(false);
				}

				if (finished)
				{
					ClearState();
				}
			}
		}

		protected virtual void OnDestroy()
		{
			DeathTokenSource.Cancel();

			if (AIManager.Instance != null)
				AIManager.Instance.RemoveAgent(this);
		}

		public void SetTarget(Transform transform)
		{
			// Can't target environmental trap.
			if (transform.TryGetComponent(out IDamageable dmg) && dmg.Team == Team.Environment)
				return;

			// Can't target self.
			if (transform == this.transform)
				return;

			Target = transform;
			TargetHealth = transform.GetComponent<Health>();
			TargetCollider = transform.GetComponent<Collider2D>();
		}

		public void StartRoom(Room room)
		{
			Room = room;
			SetTarget(GetTarget());

			IsRoomStarted = true;
			OnRoomStart();
		}

		protected virtual void OnRoomStart() { }

		protected void TransitionTo(AIState state)
		{
			if (ActiveState != state && state is not null)
			{
				ActiveState = state;
				_isActiveFirst = true;
			}
		}

		protected void ClearState()
		{
			ActiveState = null;
		}

		protected Vector2 GetRandomPositionRectInRoom()
		{
			if (Room == null)
				return Position;

			var bounds = Room.PathfindBounds.bounds;

			Vector2 center = bounds.center;
			center += new Vector2()
			{
				x = Random.Range(-bounds.extents.x, bounds.extents.x),
				y = Random.Range(-bounds.extents.y, bounds.extents.y)
			};

			return center;
		}

		protected Vector2 GetRandomPositionInRadiusInRoom()
		{
			if (Room == null)
				return Position;

			var bounds = Room.PathfindBounds.bounds;

			return (Vector2)bounds.center + (Random.insideUnitCircle * bounds.extents.y);
		}

		protected abstract void OnThink();

		protected virtual Transform GetTarget()
		{
			Transform player = PlayerManager.Instance.Player.transform;

			if (Team is Team.Monster)
			{
				return player;
			}
			else if (Team is Team.CultistA)
			{
				var target = AIManager.Instance.CultistBAgents.GetClosestTo(transform.position).transform;

				if (target == null)
				{
					target = player;
				}

				return target;
			}
			else if (Team is Team.CultistB)
			{
				var target = AIManager.Instance.CultistAAgents.GetClosestTo(transform.position).transform;

				if (target == null)
				{
					target = player;
				}

				return target;
			}

			return player;
		}

		protected virtual void OnDamaged(float amount, Vector2 dir, GameObject source)
		{
			if (source.TryGetComponent(out Health health) && health.Team != Health.Team)
			{
				SetTarget(source.transform);
			}
		}

		protected virtual void OnDeath()
		{
			DeathTokenSource.Cancel();

			AIManager.Instance.RemoveAgent(this);
			Destroy(gameObject);
		}

		protected void FacePosition(Vector2 position)
		{
			transform.localScale = new Vector3(Mathf.Sign(transform.position.DirectionTo(position).x), 1f, 1f);
		}

		protected void FaceTarget()
		{
			if (Target != null)
			{
				FacePosition(TargetPos);
			}
		}

		protected void DamageTarget(GameObject target, float damage)
		{
			if (target.TryGetComponent(out Health health))
			{
				DamageTarget(health, damage);
			}
		}
		protected void DamageTarget(Health target, float damage)
		{
			if (target.Team != Health.Team)
			{
				target.TakeDamage(damage, transform.position.DirectionTo(target.transform.position), Health.Team, gameObject);
			}
		}

		protected Vector2 GetPositionInFrontOfTarget(Vector2 target, float minDst, float maxDst, float minAngle, float maxAngle)
		{
			Vector2 selfPos = Position;
			Vector2 targetPos = target;

			float angle = Random.Range(minAngle, maxAngle);

			Vector2 targetToSelf = selfPos.DirectionTo(targetPos);
			Vector2 rotatedDir = Quaternion.AngleAxis(angle, Vector3.forward) * targetToSelf;

			float dst = Random.Range(minDst, maxDst);
			return targetPos + (rotatedDir * dst);
		}
	}
}
