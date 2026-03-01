using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.Volumes
{
	public class HurtVolume : TriggerVolume
	{
		[SerializeField, Space]
		private bool Kill;
		[SerializeField, HideIf(nameof(Kill))]
		private float Damage = 1f;
		[SerializeField]
		private Team VolumeTeam = Team.Environment;

		public override void OnEnter(Collider2D collider)
		{
			if (collider.TryGetComponent(out Health health))
			{
				if (Kill)
				{
					health.Kill();
				}
				else
				{
					health.TakeDamage(Damage, Vector2.zero, VolumeTeam, gameObject);
				}
			}
		}

		public override void OnExit(Collider2D collider) { }
	}
}
