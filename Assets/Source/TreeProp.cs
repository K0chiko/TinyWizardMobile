using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn
{
	[RequireComponent(typeof(Animator))]
	public class TreeProp : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private EventReference HitSound;
		[SerializeField, Required]
		private VisualEffect HitVFX;

		[Space, SerializeField, Range(0f, 1f)]
		private float LightsChance;
		[SerializeField]
		private SpriteRenderer[] Lights;

		public Team Team { get; } = Team.Environment;

		private Animator _animator;

		public void Awake()
		{
			_animator = GetComponent<Animator>();

			if (Random.value > LightsChance)
			{
				foreach (var light in Lights)
				{
					light.enabled = false;
				}
			}
		}

		public bool TakeDamage(DamageInfo info)
		{
			Audio.Play(HitSound, transform.position);

			float min = 0f;
			float max = 1f;

			float scale = Mathf.Lerp(min, max, info.Damage / 10f);
			scale = Mathf.Clamp(scale, min, max);

			var seq = DOTween.Sequence();
			seq.Append(transform.DOScale(1.1f * scale, 0.1f));
			seq.Append(transform.DOScale(1f, 0.2f));

			_animator.SetTrigger("Hit");

			HitVFX.SetVector2("Direction", info.Direction);
			HitVFX.Play();

			return true;
		}
	}
}
