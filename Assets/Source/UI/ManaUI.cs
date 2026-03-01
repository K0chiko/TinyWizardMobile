using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class ManaUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Slider Bar, GhostBar;

		[SerializeField, Space, Unit(Units.Second)]
		private float GhostMoveDuration = 2f;
		[SerializeField]
		private float GhostMoveDelay = 0.5f;

		private PlayerCaster _caster;

		private float _nextGhostMoveTime;
		private float _ghostPercent;
		private float _ghostVel;

		public void Start()
		{
			_caster = PlayerManager.Instance.Player.GetComponent<PlayerCaster>();

			_caster.OnManaAdded += OnManaAdded;
			_caster.OnManaRemoved += OnManaRemoved;

			_ghostPercent = 1f;
		}

		public void LateUpdate()
		{
			float realPercent = _caster.Mana / _caster.MaxMana;
			Bar.value = realPercent;

			if (Time.time > _nextGhostMoveTime)
			{
				_ghostPercent = Mathf.SmoothDamp(_ghostPercent, realPercent, ref _ghostVel, GhostMoveDuration);
				GhostBar.value = _ghostPercent;
			}
		}

		private void OnManaAdded(float amount)
		{
			float realPercent = _caster.Mana / _caster.MaxMana;

			if (realPercent > _ghostPercent)
			{
				_ghostPercent = realPercent;
			}
		}

		private void OnManaRemoved(float amount)
		{
			_nextGhostMoveTime = Time.time + GhostMoveDelay;
		}
	}
}
