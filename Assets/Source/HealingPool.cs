using FMOD;
using FMOD.Studio;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	public class HealingPool : MonoBehaviour
	{
		[SerializeField]
		private float HealInterval = 1f;
		[SerializeField]
		private float HealAmount = 1f;
		[SerializeField]
		private float SpeedFactor = 0.3f;
		[SerializeField]
		private EventReference HealingSound;

		[Space, SerializeField, Required]
		private Animator Animator;

		private float _nextHealTime;
		private bool _isHealing;

		private EventInstance _healingSound;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				_nextHealTime = Time.time + HealInterval;
				PlayerManager.Instance.Movement.ApplySpeedModifier(this, SpeedFactor);
				PlayerManager.Instance.Player.EnablePuddleMask();
			}
		}

		public void OnTriggerStay2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				bool wasHealing = _isHealing;
				_isHealing = PlayerManager.Instance.Health.Percent < 1f;

				if (_isHealing && !wasHealing)
				{
					PlayerManager.Instance.Player.OnHealingPuddleHealStart();

					_healingSound = RuntimeManager.CreateInstance(HealingSound);
					_healingSound.start();

				}
				else if (!_isHealing && wasHealing)
				{
					PlayerManager.Instance.Player.OnHealingPuddleHealEnd();

					_healingSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					_healingSound.release();
				}

				if (!_isHealing)
				{
					_nextHealTime = Time.time + HealInterval;
				}

				Animator.SetBool("IsHealing", _isHealing);

				if (Time.time > _nextHealTime && _isHealing)
				{
					var hp = PlayerManager.Instance.Health;
					hp.Heal(HealAmount);

					_nextHealTime = Time.time + HealInterval;
				}
			}
		}

		public void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				PlayerManager.Instance.Movement.RemoveSpeedModifier(this);
				PlayerManager.Instance.Player.DisablePuddleMask();

				PlayerManager.Instance.Player.OnHealingPuddleHealEnd();

				_isHealing = false;
				
				if (_healingSound.isValid())
				{
					_healingSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					_healingSound.release();
				}

				Animator.SetBool("IsHealing", false);
			}
		}
	}
}
