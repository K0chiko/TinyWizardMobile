using FMODUnity;
using Quinn.AI;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.MissileSystem
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Missile : MonoBehaviour
	{
		[SerializeField]
		private EventReference SpawnSound, HitSound, FizzleOutSound;

		[SerializeField]
		private float DirectSpeed = 8f;
		[SerializeField]
		private float DirectDamage = 1f;
		[SerializeField]
		private StatusEffect DirectStatusEffect = StatusEffect.None;
		[SerializeField]
		private float DirectStatusEffectDuration = 2f;
		[SerializeField]
		private Team Team = Team.Monster;
		[SerializeField]
		private float Lifespan = 10f;
		[SerializeField]
		private bool UsesCustomKnockbackSpeed;
		[SerializeField]
		private float CustomKnockbackSpeed;

		[SerializeField, Space]
		private GameObject SpawnOnDeath;
		[SerializeField]
		private float DestroySpawnedDelay = 3f;

		[SerializeField, Space]
		private VisualEffect[] DelayDestructionOnDeath;
		[Space, SerializeField]
		private float DestructionDelay = 3f;

		[SerializeField, Space]
		private bool IgnoreObstacles;

		[Space, SerializeField]
		private bool SwapVisuals;
		[SerializeField]
		private GameObject ChildA, ChildB;
		[SerializeField]
		private float SwapInterval = 0.5f;

		[SerializeField]
		private bool HasSplashDamage;
		[SerializeField]
		private float BaseSplashDamage = 1f;
		[SerializeField]
		private float SplashRadius = 1f;
		[SerializeField]
		private AnimationCurve SplashDamageFalloff;
		[SerializeField]
		private StatusEffect SplashStatusEffect = StatusEffect.None;
		[SerializeField]
		private float SplashStatusEffectDuration = 2f;

		[SerializeField]
		private bool DoesOscillate;
		[SerializeField]
		private float OscillateAmplitude = 0.5f;
		[SerializeField]
		private float OscillateFrequency = 0.5f;
		[SerializeField]
		private bool RandomizeOscillation;

		[SerializeField]
		private bool SpawnMissilesOnDeath;
		[SerializeField]
		private Missile DeathMissilePrefab;
		[SerializeField]
		private int DeathMissileCount = 8;
		[SerializeField]
		private MissileSpawnBehavior DeathMissileSpawnBehavior = MissileSpawnBehavior.SpreadRandom;
		[SerializeField]
		private float DeathMissileSpread = 360f;
		[SerializeField]
		private GameObject ExplosionVFX;

		[Space, SerializeField]
		private bool CreateSteam;

		private Rigidbody2D _rb;
		private GameObject _owner;

		private float _endLifeTime;
		private Vector2 _velocity;
		private Vector2 _baseDir;

		private float _oscillateOffset;

		private bool _isChildA;
		private float _nextSwapTime;

		public void Awake()
		{
			_rb = GetComponent<Rigidbody2D>();
			_oscillateOffset = RandomizeOscillation ? Random.value : 0f;
		}

		public void FixedUpdate()
		{
			_velocity = Vector2.zero;

			_velocity += UpdateDirect();
			_velocity += UpdateOscillate();

			_rb.linearVelocity = _velocity;

			if (Time.time > _endLifeTime)
			{
				OnLifespanEnd();
			}

			if (SwapVisuals && Time.time > _nextSwapTime)
			{
				_nextSwapTime = Time.time + SwapInterval;

				_isChildA = !_isChildA;

				ChildA.SetActive(_isChildA);
				ChildB.SetActive(!_isChildA);
			}
		}

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.TryGetComponent(out IDamageable dmg))
			{
				float? knockbackSpeed = null;
				if (UsesCustomKnockbackSpeed)
				{
					knockbackSpeed = CustomKnockbackSpeed;
				}

				if (dmg.TakeDamage(DirectDamage, _rb.linearVelocity.normalized, Team, _owner, DirectStatusEffect, DirectStatusEffectDuration, knockbackSpeed))
				{
					OnImpact();
				}
			}
			else if ((!IgnoreObstacles && collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")) || collision.CompareTag("MissileBlocker"))
			{
				OnImpact();

				if (CreateSteam && collision.TryGetComponent(out SteamGenerator gen))
				{
					gen.Generate();
				}
			}
		}

		public void Initialize(Vector2 dir, GameObject owner)
		{
			_baseDir = dir.normalized;
			_endLifeTime = Time.time + Lifespan;
			_owner = owner;

			Audio.Play(SpawnSound);

			if (owner != null && owner.TryGetComponent(out IAgent agent))
			{
				if (agent != null && agent.Room != null)
				{
					agent.Room.OnRoomConquered += () =>
					{
						if (this != null && gameObject != null)
						{
							OnLifespanEnd();
						}
					};
				}
			}
		}

		private void OnImpact()
		{
			Audio.Play(HitSound, transform.position);

			OnDeath();
			TriggerSplash();
			Destroy(gameObject);
		}

		private void OnLifespanEnd()
		{
			Audio.Play(FizzleOutSound, transform.position);

			OnDeath();
			TriggerSplash();
			Destroy(gameObject);
		}

		private void TriggerSplash()
		{
			if (HasSplashDamage)
			{
				var colliders = Physics2D.OverlapCircleAll(transform.position, SplashRadius);
				foreach (var collider in colliders)
				{
					if (collider.TryGetComponent(out Health health))
					{
						float dst = transform.position.DistanceTo(collider.transform.position);
						float dmg = SplashDamageFalloff.Evaluate(dst / SplashRadius) * BaseSplashDamage;
						(health as IDamageable).TakeDamage(dmg, transform.position.DirectionTo(collider.transform.position), Team, gameObject, SplashStatusEffect, SplashStatusEffectDuration);
					}
				}
			}
		}

		private Vector2 UpdateDirect()
		{
			return _baseDir * DirectSpeed;
		}

		private Vector2 UpdateOscillate()
		{
			if (!DoesOscillate)
				return Vector2.zero;

			float time = Time.time + _oscillateOffset;

			Vector2 oscDir = new Vector2(-_baseDir.y, _baseDir.x);
			return Mathf.Sin(time * OscillateFrequency) * OscillateAmplitude * oscDir;
		}

		private async void OnDeath()
		{
			if (SpawnOnDeath != null)
			{
				var instance = SpawnOnDeath.Clone(transform.position);
				Destroy(instance, DestroySpawnedDelay);
			}

			foreach (var obj in DelayDestructionOnDeath)
			{
				obj.transform.SetParent(null, true);
				obj.gameObject.Destroy(DestructionDelay);
			}

			if (SpawnMissilesOnDeath)
			{
				if (ExplosionVFX != null)
				{
					var vfx = ExplosionVFX.Clone(transform.position);
					vfx.Destroy(3f);
				}

				await MissileManager.Instance.SpawnMissileAsync(_owner, DeathMissilePrefab, transform.position, Vector2.right, DeathMissileCount, DeathMissileSpawnBehavior, DeathMissileSpread);
			}
		}
	}
}
