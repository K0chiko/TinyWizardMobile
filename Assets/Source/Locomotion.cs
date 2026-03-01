using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class Locomotion : MonoBehaviour
	{
		[FoldoutGroup("Knockback")]
		[SerializeField, Tooltip("Knockback can still be triggered manually even if this is false.")]
		private bool DoesKnockbackOnDamage = true;
		[InfoBox("@\"Knockback duration: \" + (KnockbackSpeed / KnockbackDecayRate).ToString() + 's'")]
		[SerializeField, FoldoutGroup("Knockback")]
		private float KnockbackSpeed = 12f;
		[SerializeField, FoldoutGroup("Knockback")]
		private float KnockbackDecayRate = 32f;

		protected Rigidbody2D Rigidbody { get; private set; }

		private readonly Dictionary<object, float> _speedFactors = new();

		private float _knockbackVel;
		private Vector2 _knockbackDir;

		protected virtual void Awake()
		{
			Rigidbody = GetComponent<Rigidbody2D>();

			if (DoesKnockbackOnDamage)
			{
				GetComponent<Health>().OnDamagedExpanded += OnDamaged;
			}
		}

		public void LateUpdate()
		{
			Vector2 vel = GetVelocity();

			if (_knockbackVel > 0f)
			{
				vel += _knockbackDir * _knockbackVel;
				_knockbackVel -= KnockbackDecayRate * Time.fixedDeltaTime;
			}

			foreach (var factor in _speedFactors.Values)
			{
				vel *= factor;
			}

			Rigidbody.linearVelocity = vel;
		}

		public abstract Vector2 GetVelocity();

		public void Knockback(Vector2 dir)
		{
			_knockbackVel = KnockbackSpeed;
			_knockbackDir = dir.normalized;
		}
		public void Knockback(Vector2 dir, float speed)
		{
			_knockbackVel = speed;
			_knockbackDir = dir.normalized;
		}

		public void ApplySpeedModifier(object key, float factor)
		{
			if (!_speedFactors.ContainsKey(key))
			{
				_speedFactors.Add(key, factor);
				return;
			}
			else
			{
				_speedFactors[key] = factor;
			}

			return;
		}

		public void RemoveSpeedModifier(object key)
		{
			_speedFactors.Remove(key);
		}

		private void OnDamaged(DamageInfo info)
		{
			if (info.UsesCustomKnockbackSpeed)
			{
				Knockback(info.Direction, info.CustomKnockbackSpeed);
			}
			else
			{
				Knockback(info.Direction);
			}
		}
	}
}
