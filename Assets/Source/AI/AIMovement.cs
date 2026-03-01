using Quinn.AI.Pathfinding;
using UnityEngine;

namespace Quinn.AI
{
	public class AIMovement : Locomotion
	{
		[field: SerializeField]
		public float MoveSpeed { get; set; } = 3f;
		[SerializeField]
		private int ContinuousPathfindFrameDivision = 4;
		[SerializeField]
		[Tooltip("Pathfinding will not be used below this threshold, in favor of simply moving directly to the target.")]
		private float PathfindThreshold = 2f;

		public bool IsJumping { get; private set; }

		private AIAgent _agent;
		private Vector2 _cumulativeVel;

		protected override void Awake()
		{
			base.Awake();
			_agent = GetComponent<AIAgent>();
		}

		public override Vector2 GetVelocity()
		{
			var vel = _cumulativeVel;
			_cumulativeVel = Vector2.zero;

			return vel;
		}

		public bool MoveTo(Vector2 destination, float speed, float stoppingDistance = 0.2f)
		{
			if (transform.position.DistanceTo(destination) > stoppingDistance)
			{
				_cumulativeVel += speed * (Vector2)transform.position.DirectionTo(destination);
				return false;
			}

			return true;
		}
		public bool MoveTo(Vector2 destination, float stoppingDistance = 0.2f)
		{
			return MoveTo(destination, MoveSpeed, stoppingDistance);
		}

		public async Awaitable PathTo(Vector2 target, float stoppingDistance = 0.2f)
		{
			Vector2[] path = await Pathfinder.Instance.FindPath(transform.position, target, destroyCancellationToken);
			int index = 0;

			if (path.Length == 0)
				return;

			while (index < path.Length - 1)
			{
				if (gameObject == null)
					return;

				if (transform.position.DistanceTo(target) > PathfindThreshold)
				{
					if (index < path.Length && MoveTo(path[index], stoppingDistance))
					{
						index++;
					}
				}
				else
				{
					MoveTo(target);
				}

				await Awaitable.NextFrameAsync();
			}
		}
		public async Awaitable PathTo(Transform target, float stoppingDistance = 0.2f)
		{
			if (target == null)
				return;

			int index = 0;
			Vector2[] path = await Pathfinder.Instance.FindPath(transform.position, target.position);
			Vector2 _lastTargetPos = transform.position;

			while (index < path.Length - 1 && path.Length > 0)
			{
				if (target == null || _agent.DeathTokenSource.IsCancellationRequested)
					return;

				if (transform.position.DistanceTo(target.position) > PathfindThreshold)
				{
					if (Time.frameCount % ContinuousPathfindFrameDivision == 0)
					{
						_lastTargetPos = path[index];
						path = await Pathfinder.Instance.FindPath(transform.position, target.position);
						index = 0;
					}

					if (index < path.Length && MoveTo(_lastTargetPos, stoppingDistance))
					{
						index++;

						if (index < path.Length)
						{
							_lastTargetPos = path[index];
						}
					}
				}
				else
				{
					MoveTo(target.position, stoppingDistance);
				}

				await Awaitable.NextFrameAsync();
			}
		}

		public void MoveInDirection(Vector2 direction)
		{
			_cumulativeVel += direction.normalized * MoveSpeed;
		}
		public void MoveInDirection(Vector2 direction, float speed)
		{
			_cumulativeVel += direction.normalized * speed;
		}

		public async Awaitable MoveInDirectionFor(Vector2 direction, float duration)
		{
			float endTime = Time.time + duration;

			while (Time.time < endTime && !destroyCancellationToken.IsCancellationRequested)
			{
				MoveInDirection(direction);
				await Awaitable.NextFrameAsync(destroyCancellationToken);
			}
		}
	}
}
