using Quinn.UI;
using UnityEngine;

namespace Quinn.Volumes
{
	public class SetHPVolume : TriggerVolume
	{
		[SerializeField, Space]
		private float HP = 1f;

		public override void OnEnter(Collider2D collider)
		{
			if (collider.TryGetComponent(out Health health))
			{
				health.SetCurrent(HP);

				if (collider.IsPlayer())
				{
					HeartsUI.Instance.Regenerate();
				}
			}
		}

		public override void OnExit(Collider2D collider) { }
	}
}
