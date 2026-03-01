using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.MissileSystem
{
	public class MissileManager : MonoBehaviour
	{
		public static MissileManager Instance { get; private set; }
		public static float AngleOffset = 0f;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		public void OnDestroy()
		{
			if (Instance == null)
				Instance = null;
		}

		public void SpawnMissile(GameObject owner, Missile prefab, Vector2 origin, Vector2 dir)
		{
			var instance = prefab.gameObject.Clone(origin);
			instance.GetComponent<Missile>().Initialize(dir, owner);

			MoveToActiveScene(instance);
		}

		public async Awaitable SpawnMissileAsync(GameObject owner, Missile prefab, Vector2 origin, Vector2 dir, int count,
			MissileSpawnBehavior behavior = MissileSpawnBehavior.Direct, float spreadAngle = 360f)
		{
			GameObject[] instances = await InstantiateAsync(prefab.gameObject, count, transform, origin, Quaternion.identity);

			int i = 0;
			foreach (var instance in instances)
			{
				if (instance == null)
					return;

				Vector2 missileDir = GetDirection(behavior, dir, i, spreadAngle, count);

				instance.GetComponent<Missile>().Initialize(missileDir, owner);
				MoveToActiveScene(instance);

				i++;
			}

			AngleOffset = 0f;
		}
		public async void SpawnMissile(GameObject owner, Missile prefab, Vector2 origin, Vector2 dir, int count,
			MissileSpawnBehavior behavior = MissileSpawnBehavior.Direct, float spreadAngle = 360f)
		{
			await SpawnMissileAsync(owner, prefab, origin, dir, count, behavior, spreadAngle);
		}
		public async void SpawnMissile(GameObject owner, Missile prefab, Vector2 origin, Vector2 dir, int count, float interval,
			MissileSpawnBehavior behavior = MissileSpawnBehavior.Direct, float spreadAngle = 360f)
		{
			for (int i = 0; i < count; i++)
			{
				// Occurs when scene is ended in editor while this loop is active.
				if (prefab == null)
					return;

				Vector2 missileDir = GetDirection(behavior, dir, i, spreadAngle, count);

				var instance = prefab.gameObject.Clone(origin, Quaternion.identity, transform);
				var missile = instance.GetComponent<Missile>();

				MoveToActiveScene(instance);

				missile.Initialize(missileDir, owner);
				await Awaitable.WaitForSecondsAsync(interval);
			}
		}

		private Vector2 GetDirection(MissileSpawnBehavior behavior, Vector2 baseDir, int index, float maxAngle, int count)
		{
			Vector2 missileDir = Vector2.zero;

			switch (behavior)
			{
				case MissileSpawnBehavior.Direct:
				{
					missileDir = baseDir;
					break;
				}
				case MissileSpawnBehavior.SpreadRandom:
				{
					float angle = Random.Range(0, maxAngle);
					angle -= maxAngle / 2f;

					missileDir = Quaternion.AngleAxis(angle, Vector3.forward) * baseDir;
					break;
				}
				case MissileSpawnBehavior.SpreadEven:
				{
					float angleDelta = maxAngle / count;

					float angle = index * angleDelta;
					angle -= maxAngle / 2f;

					angle += AngleOffset;

					missileDir = Quaternion.AngleAxis(angle, Vector3.forward) * baseDir;
					break;
				}
			}

			return missileDir;
		}

		private static void MoveToActiveScene(GameObject instance)
		{
			instance.transform.SetParent(null, true);
			SceneManager.MoveGameObjectToScene(instance, SceneManager.GetActiveScene());
		}
	}
}
