using FMODUnity;
using Quinn.AI;
using Sirenix.OdinInspector;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.MissileSystem
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Missile : MonoBehaviour
	{
		[SerializeField, FoldoutGroup("SFX")]
		private EventReference SpawnSound, HitSound, FizzleOutSound;

		[SerializeField, BoxGroup("Core")]
		private float DirectSpeed = 8f;
		[SerializeField, BoxGroup("Core")]
		private float DirectDamage = 1f;
		[SerializeField, BoxGroup("Core")]
		private StatusEffect DirectStatusEffect = StatusEffect.None;
		[SerializeField, BoxGroup("Core")]
		private float DirectStatusEffectDuration = 2f;
		[SerializeField, BoxGroup("Core")]
		private Team Team = Team.Monster;
		[SerializeField, BoxGroup("Core"), Unit(Units.Second)]
		private float Lifespan = 10f;
		[SerializeField]
		private bool UsesCustomKnockbackSpeed;
		[SerializeField, ShowIf(nameof(UsesCustomKnockbackSpeed))]
		private float CustomKnockbackSpeed;

		[SerializeField, BoxGroup("Core"), Space]
		private GameObject SpawnOnDeath;
		[SerializeField, BoxGroup("Core"), ShowIf(nameof(SpawnOnDeath))]
		private float DestroySpawnedDelay = 3f;

		[SerializeField, BoxGroup("Core"), Space]
		private VisualEffect[] DelayDestructionOnDeath;
		[Space, SerializeField, BoxGroup("Core"), ShowIf("@DelayDestructionOnDeath.Length > 0"), Unit(Units.Second)]
		private float DestructionDelay = 3f;

		[SerializeField, BoxGroup("Core"), Space]
		private bool IgnoreObstacles;

		[Space, SerializeField, FoldoutGroup("Flicker")]
		private bool SwapVisuals;
		[SerializeField, ShowIf(nameof(SwapInterval)), FoldoutGroup("Flicker")]
		private GameObject ChildA, ChildB;
		[SerializeField, ShowIf(nameof(SwapInterval)), FoldoutGroup("Flicker")]
		private float SwapInterval = 0.5f;

		[SerializeField, FoldoutGroup("Splash Damage")]
		private bool HasSplashDamage;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private float BaseSplashDamage = 1f;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private float SplashRadius = 1f;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private AnimationCurve SplashDamageFalloff;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private StatusEffect SplashStatusEffect = StatusEffect.None;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private float SplashStatusEffectDuration = 2f;

		[SerializeField, FoldoutGroup("Oscillate")]
		private bool DoesOscillate;
		[SerializeField, FoldoutGroup("Oscillate"), ShowIf(nameof(DoesOscillate))]
		private float OscillateAmplitude = 0.5f;
		[SerializeField, FoldoutGroup("Oscillate"), ShowIf(nameof(DoesOscillate))]
		private float OscillateFrequency = 0.5f;
		[SerializeField, FoldoutGroup("Oscillate"), ShowIf(nameof(DoesOscillate))]
		private bool RandomizeOscillation;

		[SerializeField, FoldoutGroup("Explosion")]
		private bool SpawnMissilesOnDeath;
		[SerializeField, FoldoutGroup("Explosion"), ShowIf(nameof(SpawnMissilesOnDeath)), Required]
		private Missile DeathMissilePrefab;
		[SerializeField, FoldoutGroup("Explosion"), ShowIf(nameof(SpawnMissilesOnDeath))]
		private int DeathMissileCount = 8;
		[SerializeField, FoldoutGroup("Explosion"), ShowIf(nameof(SpawnMissilesOnDeath))]
		private MissileSpawnBehavior DeathMissileSpawnBehavior = MissileSpawnBehavior.SpreadRandom;
		[SerializeField, FoldoutGroup("Explosion"), ShowIf(nameof(SpawnMissilesOnDeath))]
		private float DeathMissileSpread = 360f;
		[SerializeField, FoldoutGroup("Explosion")]
		private GameObject ExplosionVFX;

		[Space, SerializeField, FoldoutGroup("Misc")]
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
