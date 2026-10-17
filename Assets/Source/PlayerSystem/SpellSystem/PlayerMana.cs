using System;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem
{
	public sealed class PlayerMana
	{
		public float Current { get; private set; }
		public float Max { get; }

		public event Action<float> Added, Removed;

		private readonly float _regenDelay;
		private readonly float _regenRate;
		private float _regenStartTime;

		public PlayerMana(float max, float regenDelay, float regenRate)
		{
			Max = max;
			_regenDelay = regenDelay;
			_regenRate = regenRate;
		}

		public void SetFull()
		{
			Current = Max;
		}

		public void Tick(bool canRegen)
		{
			if (Time.time > _regenStartTime && Current < Max && canRegen)
			{
				Current = Mathf.Min(Current + (Time.deltaTime * _regenRate), Max);
			}
		}

		public void Consume(float amount)
		{
			if (Current == 0f)
				return;

			Current = Mathf.Max(0f, Current - amount);

			if (amount > 0f)
			{
				_regenStartTime = Time.time + _regenDelay;
			}

			Removed?.Invoke(amount);
		}

		public void Replenish(float amount)
		{
			if (Current >= Max)
				return;

			Current = Mathf.Min(Current + amount, Max);
			Added?.Invoke(amount);
		}

		public void ReplenishFully()
		{
			Replenish(Max);
		}
	}
}
