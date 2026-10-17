using DG.Tweening;
using FMODUnity;
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

		[SerializeField]
		private float FlashInDuration = 0.1f, FlashHoldDuration = 0.05f, FlashOutDuration = 0.1f;
		[SerializeField]
		private Ease FlashInEase = Ease.Linear, FlashOutEase = Ease.Linear;

		[SerializeField, Space]
		private bool IsImmuneOnHurt;
		[SerializeField]
		private float HurtImmunityDuration = 2f;
		[SerializeField]
		private int BlinkCount = 3;
		[SerializeField]
		private float BlinkInterval = 0.1f;

		[Space, SerializeField]
		private bool HasHBBar = true;
		[SerializeField]
		private Slider HPBar;
		[SerializeField]
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

		private bool _isHurtImmune;
		private readonly HashSet<object> _damageBlockers = new();
		private float _nextHurtImmunityEndTime;

		private StatusEffectManager _statsEffectManager;
		private HurtFeedback _hurtFeedback;

		private bool _lastHPBarActive;
		private float _lastHPBarValue = -1f;

		public void Awake()
		{
			Current = Max;
			TryGetComponent(out _statsEffectManager);

			_hurtFeedback = new HurtFeedback(
				this, Renderers,
				FlashInDuration, FlashHoldDuration, FlashOutDuration,
				FlashInEase, FlashOutEase,
				BlinkCount, BlinkInterval,
				() => IsImmuneOnHurt && !IsDead);
		}

		public void Start()
		{
			if (HPBar != null)
			{
				HPBar.transform.parent.SetParent(null, true);
				_lastHPBarActive = HPBar.gameObject.activeSelf;
			}
		}

		public void FixedUpdate()
		{
			if (HPBar != null)
			{
				bool shouldBeActive = HasHBBar && Current < Max && IsAlive;
				if (shouldBeActive != _lastHPBarActive)
				{
					HPBar.gameObject.SetActive(shouldBeActive);
					HPArrow.gameObject.SetActive(shouldBeActive);
					_lastHPBarActive = shouldBeActive;
				}

				float value = Current / Max;
				if (!Mathf.Approximately(value, _lastHPBarValue))
				{
					HPBar.value = value;
					_lastHPBarValue = value;
				}
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

			_hurtFeedback.Play();
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

		private void DestroyHPBar()
		{
			if (HPBar != null)
			{
				HPBar.transform.parent.gameObject.Destroy();
			}
		}
	}
}
