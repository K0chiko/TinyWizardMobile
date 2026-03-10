using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace Quinn
{
	public class BreakableWall : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private float HP = 100f;

		[SerializeField, Space]
		private EventReference BreakSound, AdditionalSoundOnBreak;
		[SerializeField]
		private GameObject VFX;
		[SerializeField]
		private float VFXDestroyDelay = 5f;

		[Space, SerializeField]
		private UnityEvent OnBreak;

		public Team Team => Team.Environment;

		private float _hp;

		public void Awake()
		{
			_hp = HP;
		}

		public bool TakeDamage(DamageInfo info)
		{
			if (_hp <= 0)
				return false;

			_hp -= info.Damage;

			if (_hp <= 0)
			{
				Audio.Play(BreakSound, transform.position);
				Audio.Play(AdditionalSoundOnBreak, transform.position);

				VFX.transform.SetParent(null, true);
				VFX.GetComponent<VisualEffect>().Play();
				VFX.Destroy(VFXDestroyDelay);

				OnBreak?.Invoke();
				Destroy(gameObject);
			}

			return true;
		}
	}
}
