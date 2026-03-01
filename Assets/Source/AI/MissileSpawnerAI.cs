using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class MissileSpawnerAI : MonoBehaviour
	{
		[SerializeField]
		private Vector2 Direction;

		[SerializeField, Required]
		private Missile Missile;
		[SerializeField]
		private int Count = 1;
		[SerializeField, ShowIf("@Count > 1")]
		private float Interval = 0.2f;
		[SerializeField]
		private MissileSpawnBehavior Behavior = MissileSpawnBehavior.Direct;
		[SerializeField, ShowIf("@Behavior != MissileSpawnBehavior.Direct")]
		private float Spread = 30f;

		[Space, SerializeField]
		private bool AutoFire;
		[SerializeField, ShowIf(nameof(AutoFire))]
		private float FireInterval = 2f;

		private float _nextFireTime;

		public void Update()
		{
			if (AutoFire && Time.time > _nextFireTime)
			{
				_nextFireTime = Time.time + FireInterval + ((Count - 1) * Interval);
				Fire();
			}
		}

		public void Fire()
		{
			if (Count == 1)
			{
				MissileManager.Instance.SpawnMissile(gameObject, Missile, transform.position, Direction.normalized, Count, Behavior, Spread);
			}
			else
			{
				MissileManager.Instance.SpawnMissile(gameObject, Missile, transform.position, Direction.normalized, Count, Interval, Behavior, Spread);
			}
		}

		public void CeaseAutoFire()
		{
			AutoFire = false;
		}
	}
}
