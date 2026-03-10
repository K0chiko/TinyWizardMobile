using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace Quinn
{
	public class Health : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private EventReference HurtSound, DeathSound, HealSound;
		[field: SerializeField]
		public float Max { get; private set; } = 100f;
		[field: SerializeField]
		public Team Team { get; private set; }
		[SerializeField]
		private SpriteRenderer[] Renderers;
		[SerializeField]
		private VisualEffect HitVFX;

		[SerializeField, FoldoutGroup("Hurt Flash")]
		private float FlashInDuration = 0.1f, FlashHoldDuration = 0.05f, FlashOutDuration = 0.1f;
		[SerializeField, FoldoutGroup("Hurt Flash")]
		private Ease FlashInEase = Ease.Linear, FlashOutEase = Ease.Linear;

		[SerializeField, Space]
		private bool IsImmuneOnHurt;
		[SerializeField, FoldoutGroup("Hurt Immunity"), ShowIf(nameof(IsImmuneOnHurt))]
		private float HurtImmunityDuration = 2f;
		[SerializeField, FoldoutGroup("Hurt Immunity"), ShowIf(nameof(IsImmuneOnHurt))]
		[InfoBox("@\"Total blink animation time: \" + (BlinkCount * BlinkInterval * 2f).ToString() + 's'")]
		private int BlinkCount = 3;
		[SerializeField, FoldoutGroup("Hurt Immunity"), ShowIf(nameof(IsImmuneOnHurt))]
		private float BlinkInterval = 0.1f;

		[Space, SerializeField]
		private bool HasHBBar = true;
		[SerializeField, ShowIf(nameof(HasHBBar))]
		private Slider HPBar;
		[SerializeField, ShowIf("@HPBar && HasHBBar")]
		private Image HPArrow;

		public float Current { get; private set; }
		public float Percent => Current / Max;

		public bool IsAlive => !IsDead;
		public bool IsDead { get; private set; }
		public bool IsImmune => _isHurtImmune || _damageBlockers.Count > 0;

		public bool IsGodModeEnabled { get; set; }

		public event Action<float> OnHealed;
		public event Action<float, Vector2, GameObject> OnDamaged;
		public event Action<DamageInfo> OnDamagedExpanded;
		public event Action OnDeath;
		public event Action OnMaxChange;

		private bool _isHurtImmune;
		private readonly HashSet<object> _damageBlockers = new();
		private float _nextHurtImmunityEndTime;

		private StatusEffectManager _statsEffectManager;

		public void Awake()
		{
			Current = Max;
			TryGetComponent(out _statsEffectManager);
		}

		public void Start()
		{
			if (HPBar != null)
			{
				HPBar.transform.parent.SetParent(null, true);
			}
		}

		public void FixedUpdate()
		{
			if (HPBar != null)
			{
				HPBar.gameObject.SetActive(HasHBBar && Current < Max && IsAlive);
				HPBar.value = Current / Max;

				HPArrow.gameObject.SetActive(HPBar.gameObject.activeInHierarchy);
			}
		}

		public void OnDestroy()
		{
			DestroyHPBar();
		}

		public void SetCurrent(float value)
		{
			Current = Mathf.Clamp(value, 0f, Max);
		}

		public void Heal(float health)
		{
			if (IsDead)
				return;

			Current += health;
			Current = Mathf.Min(Current, Max);

			OnHealed?.Invoke(health);

			Audio.Play(HealSound, transform.position);
		}

		public void FullHeal()
		{
			Heal(Max - Current);
		}

		public bool TakeDamage(float damage, Vector2 dir, Team sourceTeam, GameObject source, float? customKnockback = null)
		{
			var info = new DamageInfo()
			{
				Damage = damage,
				Direction = dir,
				SourceTeam = sourceTeam,
				Source = source,
				UsesCustomKnockbackSpeed = customKnockback.HasValue
			};

			if (customKnockback.HasValue)
			{
				info.CustomKnockbackSpeed = customKnockback.Value;
			}

			return TakeDamage(info);
		}
		public bool TakeDamage(DamageInfo info)
		{
#if UNITY_EDITOR
			if (IsGodModeEnabled)
				return false;
#endif

			if (Time.time >= _nextHurtImmunityEndTime)
				_isHurtImmune = false;

			if (IsDead || info.SourceTeam == Team || IsImmune)
				return false;

			Audio.Play(HurtSound, transform.position);

			if (_statsEffectManager != null)
			{
				_statsEffectManager.ApplyEffect(info.StatusEffect, info.StatusEffectDuration);
			}

			if (HitVFX != null)
			{
				if (HitVFX.HasVector2("Direction"))
				{
					HitVFX.SetVector2("Direction", info.Source.transform.position.DirectionTo(transform.position));
				}

				HitVFX.Play();
			}

			Current -= info.Damage;
			Current = Mathf.Max(0, Current);

			if (Current == 0)
			{
				info.IsLethal = true;
			}

			OnDamaged?.Invoke(info.Damage, info.Direction.normalized, info.Source);
			OnDamagedExpanded?.Invoke(info);

			if (Current == 0f)
			{
				Audio.Play(DeathSound, transform.position);

				IsDead = true;
				OnDeath?.Invoke();

				DestroyHPBar();
			}

			if (IsImmuneOnHurt && !IsDead)
			{
				_isHurtImmune = true;
				_nextHurtImmunityEndTime = Time.time + HurtImmunityDuration;
			}

			AnimateHurtVFX();
			return true;
		}

		public void Kill()
		{
			TakeDamage(Max + 1f, Vector2.zero, Team.Environment, gameObject);
		}

		public void BlockDamage(object key)
		{
			_damageBlockers.Add(key);
		}
		public void UnblockDamage(object key)
		{
			_damageBlockers.Remove(key);
		}

		public void SetMax(float max)
		{
			Max = max;
			OnMaxChange?.Invoke();
		}

		private async void AnimateHurtVFX()
		{
			foreach (var renderer in Renderers)
			{
				Debug.Assert(renderer != null, $"Health damaged FX found a null sprite renderered! Make sure '{gameObject.name}' has no missing references on its Health component.");

				renderer.enabled = true;
				renderer.material.SetFloat("_Flash", 0f);
			}

			await AnimateFlash();

			if (IsImmuneOnHurt && !IsDead)
			{
				AnimateBlink();
			}
		}

		private async Awaitable AnimateFlash()
		{
			float Get()
			{
				if (Renderers.Length == 0) 
					return 0f;

				var renderer = Renderers[0];

				if (renderer == null)
					return 0f;

				return renderer.material.GetFloat("_Flash");
			}
			void Set(float value)
			{
				if (this == null)
					return;

				foreach (var renderer in Renderers)
				{
					if (renderer == null)
						return;

					renderer.material.SetFloat("_Flash", value);
				}
			}

			if (Renderers.Length > 0 && this != null)
			{
				await DOTween.To(Get, Set, 1f, FlashInDuration)
					.SetEase(FlashInEase)
					.AsyncWaitForCompletion();

				await Awaitable.WaitForSecondsAsync(FlashHoldDuration);

				await DOTween.To(Get, Set, 0f, FlashOutDuration)
					.SetEase(FlashOutEase)
					.AsyncWaitForCompletion();
			}
		}

		private async void AnimateBlink()
		{
			for (int i = 0; i < BlinkCount; i++)
			{
				await Awaitable.WaitForSecondsAsync(BlinkInterval);

				foreach (var renderer in Renderers)
				{
					renderer.enabled = false;
				}

				await Awaitable.WaitForSecondsAsync(BlinkInterval);

				foreach (var renderer in Renderers)
				{
					renderer.enabled = true;
				}
			}
		}

		private void DestroyHPBar()
		{
			if (HPBar != null)
			{
				HPBar.transform.parent.gameObject.Destroy();
			}
		}
	}
}
