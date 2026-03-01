using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn
{
	[RequireComponent(typeof(Locomotion))]
	[RequireComponent(typeof(Health))]
	public class StatusEffectManager : MonoBehaviour
	{
		[SerializeField]
		private List<StatusEffect> Immune;

		[SerializeField, FoldoutGroup("Burning"), Required]
		private VisualEffect BurningVFX;
		[SerializeField, FoldoutGroup("Burning"), Required]
		private float BurnInterval = 1f;
		[SerializeField, FoldoutGroup("Burning")]
		private float BurnDamage = 5f;

		[SerializeField, Required, FoldoutGroup("Wet")]
		private VisualEffect WetVFX;
		[SerializeField, FoldoutGroup("Wet")]
		private float WetSpeedFactor = 0.5f;
		[SerializeField, FoldoutGroup("Wet")]
		private float WetDamageInterval = 1f;
		[SerializeField, FoldoutGroup("Wet")]
		private float WetDamage = 0f;

		private Locomotion _movement;
		private Health _health;

		private readonly Dictionary<StatusEffect, float> _effects = new();
		private float _nextBurnDamageTime;
		private float _nextWetDamageTime;

		public void Awake()
		{
			_movement = GetComponent<Locomotion>();
			_health = GetComponent<Health>();

			_health.OnDeath += OnDeath;
		}

		public void FixedUpdate()
		{
			var toRemove = new HashSet<StatusEffect>();

			foreach (var effect in _effects)
			{
				if (Time.time > effect.Value)
				{
					toRemove.Add(effect.Key);
				}
			}

			foreach (var effect in  toRemove)
			{
				RemoveEffect(effect);
			}

			if (HasEffect(StatusEffect.Burning) && Time.time > _nextBurnDamageTime)
			{
				_nextBurnDamageTime = Time.time + BurnInterval;
				_health.TakeDamage(BurnDamage, Vector2.zero, Team.Environment, gameObject, 0f);
			}

			if (HasEffect(StatusEffect.Wet) && Time.time > _nextWetDamageTime && WetDamage > 0f)
			{
				_nextWetDamageTime = Time.time + WetDamageInterval;
				_health.TakeDamage(WetDamage, Vector2.zero, Team.Environment, gameObject, 0f);
			}
		}

		public void ClearAll()
		{
			var toRemove = new HashSet<StatusEffect>(_effects.Keys);

			foreach (var effect in toRemove)
			{
				RemoveEffect(effect);
			}
		}

		public bool HasEffect(StatusEffect effect)
		{
			return _effects.ContainsKey(effect);
		}

		public void ApplyEffect(StatusEffect effect, float duration)
		{
			if (effect is StatusEffect.None)
				return;

			if (Immune.Contains(effect))
				return;

			if (HasEffect(effect))
			{
				_effects[effect] = Time.time + duration;
				return;
			}

			_effects.Add(effect, Time.time + duration);
			EffectToVFX(effect).Play();

			switch (effect)
			{
				case StatusEffect.Burning:
				{
					_nextBurnDamageTime = Time.time + BurnInterval;
					break;
				}
				case StatusEffect.Wet:
				{
					_movement.ApplySpeedModifier(WetVFX, WetSpeedFactor);
					_nextWetDamageTime = Time.time + WetDamageInterval;
					break;
				}
			}
		}

		public void RemoveEffect(StatusEffect effect)
		{
			if (HasEffect(effect))
			{
				_effects.Remove(effect);
				EffectToVFX(effect).Stop();

				switch (effect)
				{
					case StatusEffect.Wet:
					{
						_movement.RemoveSpeedModifier(WetVFX);
						break;
					}
				}
			}
		}

		private VisualEffect EffectToVFX(StatusEffect effect) => effect switch
		{
			StatusEffect.Burning => BurningVFX,
			StatusEffect.Wet => WetVFX,
			_ => throw new System.NotImplementedException()
		};

		private void OnDeath()
		{
			ClearAll();
		}
	}
}
