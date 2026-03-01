using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	public class Spikes : MonoBehaviour
	{
		[SerializeField]
		private float PlayerDamage = 1f, EnemyDamage = 5f;
		[SerializeField]
		private float KnockbackSpeed = 12f;
		[SerializeField, Required]
		private Collider2D Bounds;

		private readonly HashSet<Collider2D> _damaged = new();

		public void OnCollisionStay2D(Collision2D collision)
		{
			if (_damaged.Contains(collision.collider))
				return;

			bool damageSuccess = false;

			if (collision.gameObject.TryGetComponent(out Health health))
			{
				float dmg = PlayerDamage;
				if (health.Team != Team.Player)
				{
					dmg = EnemyDamage;
				}

				damageSuccess = health.TakeDamage(dmg, Bounds.bounds.center.DirectionTo(health.transform.position), Team.Environment, gameObject, KnockbackSpeed);
			}

			if (damageSuccess)
			{
				_damaged.Add(collision.collider);
			}
		}

		public void OnCollisionExit2D(Collision2D collision)
		{
			_damaged.Remove(collision.collider);
		}
	}
}
