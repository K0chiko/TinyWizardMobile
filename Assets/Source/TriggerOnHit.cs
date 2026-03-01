using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace Quinn
{
	public class TriggerOnHit : MonoBehaviour, IDamageable
	{
		[field: SerializeField]
		public Team Team { get; private set; }
		[SerializeField]
		private bool ConsumeHit = true;

		[Space, SerializeField]
		private EventReference HitSound;
		[SerializeField]
		private UnityEvent OnHit;

		public bool TakeDamage(DamageInfo info)
		{
			Audio.Play(HitSound, transform.position);

			OnHit?.Invoke();
			return ConsumeHit;
		}
	}
}
