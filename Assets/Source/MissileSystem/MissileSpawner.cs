using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.MissileSystem
{
	public class MissileSpawner : MonoBehaviour
	{
		[SerializeField, Required]
		private Missile Prefab;
		[SerializeField]
		private int Count = 1;
		[SerializeField]
		private MissileSpawnBehavior Behavior;
		[SerializeField, HideIf(nameof(Behavior), MissileSpawnBehavior.Direct)]
		private float MaxAngle = 360f;
		[SerializeField]
		private float PerMissileInterval;

		[Space, SerializeField]
		private float StartDelay = 0f;
		[SerializeField]
		private float FireInterval = 1f;
		[SerializeField]
		private Vector2 InitialDirection = Vector2.up;
		[SerializeField]
		private float RotationRate = 0f;

		private Vector2 _dir;

		public async void Start()
		{
			_dir = InitialDirection.normalized;
			await Awaitable.WaitForSecondsAsync(StartDelay);

			while (true)
			{
				MissileManager.Instance.SpawnMissile(gameObject, Prefab, transform.position, _dir, Count, PerMissileInterval, Behavior, MaxAngle);
				await Awaitable.WaitForSecondsAsync(FireInterval);
			}
		}

		public void Update()
		{
			if (RotationRate != 0f)
			{
				_dir = Quaternion.AngleAxis(RotationRate * Time.deltaTime, Vector3.forward) * _dir;
			}
		}
	}
}
