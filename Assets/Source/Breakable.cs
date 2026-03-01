using FMODUnity;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn
{
	public class Breakable : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private Sprite[] DestroyedSprites;
		[SerializeField, Tooltip("Played on destruction.")]
		private VisualEffect[] VFX;
		[SerializeField]
		private EventReference DamageSound, DestroySound;
		[SerializeField]
		private float DestructionDelay = 5f;

		public Team Team => Team.Environment;

		private int _hits;
		private bool _isDead;

		public bool TakeDamage(DamageInfo info)
		{
			if (_isDead)
				return false;

			if (DestroyedSprites.Length > 0)
			{
				if (_hits >= DestroyedSprites.Length)
				{
					Death();
					return true;
				}
				else
				{
					Audio.Play(DamageSound, transform.position);
				}

				GetComponent<SpriteRenderer>().sprite = DestroyedSprites[_hits];
				_hits++;
			}
			else
			{
				Death();
				_isDead = true;
			}

			return true;
		}

		private void Death()
		{

			GetComponent<SpriteRenderer>().enabled = false;

			foreach (var collider in GetComponents<Collider2D>())
			{
				collider.enabled = false;
			}

			foreach (var vfx in VFX)
			{
				vfx.Play();
			}

			Audio.Play(DestroySound, transform.position);
			Destroy(gameObject, DestructionDelay);
		}
	}
}
