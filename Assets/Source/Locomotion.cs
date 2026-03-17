using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quinn
{
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class Locomotion : MonoBehaviour
	{
		[FormerlySerializedAs("DoesKnockbackOnDamage")] [SerializeField, Tooltip("Knockback can still be triggered manually even if this is false.")]
		private bool doesKnockbackOnDamage = true;
		[FormerlySerializedAs("KnockbackSpeed")] [SerializeField]
		private float knockbackSpeed = 12f;
		[FormerlySerializedAs("KnockbackDecayRate")] [SerializeField]
		private float knockbackDecayRate = 32f;

		protected Rigidbody2D Rigidbody { get; private set; }

		private readonly Dictionary<object, float> _speedFactors = new();

		private float _knockbackVel;
		private Vector2 _knockbackDir;

		protected virtual void Awake()
		{
			Rigidbody = GetComponent<Rigidbody2D>();

			if (doesKnockbackOnDamage)
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
				_knockbackVel -= knockbackDecayRate * Time.fixedDeltaTime;
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
			_knockbackVel = knockbackSpeed;
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
