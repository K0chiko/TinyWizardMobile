using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Health))]
	public class DamageOnContact : MonoBehaviour
	{
		[SerializeField]
		private float Damage = 1f;

		private Health _health;

		public void Awake()
		{
			_health = GetComponent<Health>();
		}

		public void OnCollisionEnter2D(Collision2D collision)
		{
			if (!enabled)
				return;

			if (collision.gameObject.TryGetComponent(out Health health))
			{
				health.TakeDamage(Damage, transform.position.DirectionTo(collision.transform.position), _health.Team, gameObject);
			}
		}
	}
}
