using UnityEngine;

namespace Quinn
{
	public record DamageInfo
	{
		public float Damage;
		public Vector2 Direction;
		public Team SourceTeam;
		public GameObject Source;
		public bool UsesCustomKnockbackSpeed;
		public float CustomKnockbackSpeed;
		public bool IsLethal;
		public StatusEffect StatusEffect;
		public float StatusEffectDuration;
	}
}
