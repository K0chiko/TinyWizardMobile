using UnityEngine;

namespace Quinn.EvolutionLearning
{
	public class Agent : MonoBehaviour
	{
		[SerializeField]
		private float MoveSpeed = 1f;

		public Playground Playground { get; set; }
		public Transform Goal { get; set; }

		public Brain Brain { get; set; }

		private float _failTime;
		private bool _isActive = true;

		private SpriteRenderer _renderer;

		private void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
			_failTime = Time.time + 10f;
		}

		private void Update()
		{
			if (!_isActive)
				return;

			float goalDir = transform.position.DirectionTo(Goal.position).x;
			float moveDir = Mathf.Sign(Brain.Evaluate(goalDir));

			transform.Translate(moveDir * MoveSpeed * Time.deltaTime * Vector2.right);

			if (transform.position.DistanceTo(Goal.position) < 0.2f)
			{
				End(true);
			}
			else if (Time.time > _failTime || transform.position.x < -12f || transform.position.x > 12f)
			{
				End(false);
			}

			float clampedX = Mathf.Clamp(transform.position.x, -12.5f, 12.5f);
			transform.position = new(clampedX, transform.position.y);
		}

		public void End(bool success)
		{
			if (!_isActive)
				return;

			_isActive = false;

			Brain.Cost = transform.position.DistanceTo(Goal.position);
			Playground.EndEpisode(success);

			_renderer.color = Color.Lerp(Color.green, Color.red, Brain.Cost / 25f);
		}
	}
}
