using FMODUnity;
using UnityEngine;

namespace Quinn.AI
{
	public class SlimeAI : AIAgent
	{
		[SerializeField]
		private float JumpPrimeDuration = 0.5f;
		[SerializeField]
		private Vector2 JumpInterval = new(0.1f, 0.4f);
		[SerializeField]
		private float JumpDistance = 2f;

		[Space, SerializeField]
		private EventReference JumpSound;
		[SerializeField]
		private EventReference LandSound;

		[Space, SerializeField]
		private bool DisableAI;

		protected override void OnThink() { }

		protected override async void OnRoomStart()
		{
			if (DisableAI)
				return;

			while (!DeathTokenSource.IsCancellationRequested)
			{
				FaceTarget();
				await Wait.Seconds(JumpInterval.GetRandom(), DeathTokenSource.Token);
				FaceTarget();

				Vector2 dest = transform.position;
				dest += Position.DirectionTo(TargetPos) * JumpDistance;
				await Jump(dest);

				FaceTarget();
			}
		}

		private async Awaitable Jump(Vector2 destination)
		{
			Animator.SetTrigger("PrimeJump");
			await Wait.Seconds(JumpPrimeDuration, DeathTokenSource.Token);
			Animator.SetTrigger("Jump");

			Audio.Play(JumpSound, transform.position);

			float speed = JumpDistance;
			float dst = transform.position.DistanceTo(destination);

			float dur = dst / speed;
			float endTime = Time.time + dur;

			while (Time.time < endTime && gameObject != null && !DeathTokenSource.IsCancellationRequested)
			{
				Movement.MoveInDirection(transform.position.DirectionTo(destination), speed);
				await Wait.NextFrame(DeathTokenSource.Token);
			}

			Audio.Play(LandSound, transform.position);
		}

		protected override void OnDeath()
		{
			DeathTokenSource.Cancel();
			AIManager.Instance.RemoveAgent(this);

			Animator.SetTrigger("Die");
			GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
		}
	}
}
