using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn
{
	public class Foliage : MonoBehaviour, IDamageable
	{
		[SerializeField, Required]
		private VisualEffect DestroyVFX;
		[SerializeField]
		private float DestroyDelay = 5f;

		[Space, SerializeField]
		private EventReference TouchSound;
		[SerializeField]
		private EventReference DestroySound;

		public Team Team => Team.Environment;

		public void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.IsCharacter())
			{
				Audio.Play(TouchSound, transform.position);
			}
		}

		public bool TakeDamage(DamageInfo info)
		{
			if (info.SourceTeam != Team.Player)
				return false;

			Audio.Play(DestroySound, transform.position);

			GetComponent<Collider2D>().enabled = false;
			GetComponent<SpriteRenderer>().enabled = false;

			DestroyVFX.Play();
			Destroy(gameObject, DestroyDelay);

			return false;
		}
	}
}
