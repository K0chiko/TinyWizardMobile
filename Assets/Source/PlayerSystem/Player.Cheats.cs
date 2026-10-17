using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quinn.PlayerSystem
{
	public partial class Player
	{
		private bool _noClip;
		private int _preNoClipMask;
		private bool _godMode;

		[Conditional("UNITY_EDITOR")]
		private void ApplyCheats()
		{
			var keyboard = Keyboard.current;
			if (keyboard == null)
				return;

			if (keyboard.digit8Key.wasPressedThisFrame)
			{
				GetComponent<Health>().TakeDamage(1f, Vector2.zero, Team.Environment, gameObject);
			}

			if (keyboard.vKey.wasPressedThisFrame)
			{
				_noClip = !_noClip;

				if (_noClip)
				{
					var collider = GetComponent<Collider2D>();
					_preNoClipMask = collider.forceReceiveLayers;
					collider.forceReceiveLayers = 0;

					GetComponent<PlayerMovement>().SetSpeedOverride(16f);
				}
				else
				{
					GetComponent<Collider2D>().forceReceiveLayers = _preNoClipMask;
					GetComponent<PlayerMovement>().ClearSpeedOverride();
				}
			}

			if (keyboard.gKey.wasPressedThisFrame)
			{
				_godMode = !_godMode;
				GetComponent<Health>().IsGodModeEnabled = _godMode;
			}
		}
	}
}
