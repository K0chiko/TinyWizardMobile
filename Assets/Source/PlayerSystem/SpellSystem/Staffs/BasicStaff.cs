using FMODUnity;
using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class BasicStaff : Staff
	{
		[SerializeField, FoldoutGroup("SFX")]
		private EventReference BasicCastSound;
		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("SFX")]
		private EventReference BasicFinisherCastSound;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("SFX")]
		private EventReference SpecialCastLittleSound, SpecialCastBigSound, FullChargeSound;

		[SerializeField, Required, FoldoutGroup("Basic")]
		private Missile BasicMissile;
		[SerializeField, FoldoutGroup("Basic"), Unit(Units.Second)]
		private float BasicCooldown = 0.3f;
		[SerializeField, FoldoutGroup("Basic"), Unit(Units.MetersPerSecond)]
		private float BasicKnockbackSpeed = 10f;

		[SerializeField, FoldoutGroup("Basic"), Space]
		private MissileSpawnBehavior BasicBehavior = MissileSpawnBehavior.Direct;
		[SerializeField, HideIf("@BasicBehavior == MissileSpawnBehavior.Direct"), FoldoutGroup("Basic"), Unit(Units.Degree)]
		private float BasicSpread = 0f;
		[SerializeField, FoldoutGroup("Basic")]
		private int BasicCount = 1;
		[SerializeField, ShowIf("@BasicCount > 1"), FoldoutGroup("Basic"), Unit(Units.Second)]
		private float BasicInterval = 0f;
		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("Basic"), Unit(Units.Second)]
		private float ChainWindowDuration = 0.4f;

		[SerializeField, FoldoutGroup("Basic"), Space]
		private float BasicEnergyUse = 2f;
		[SerializeField, FoldoutGroup("Basic")]
		private float BasicManaConsume = 4f;

		[Space, SerializeField, FoldoutGroup("Basic Finisher")]
		private bool HasBasicFinisher = true;
		[SerializeField, FoldoutGroup("Basic Finisher"), Unit(Units.Second), ShowIf(nameof(HasBasicFinisher))]
		private float BasicFinisherCooldown = 0.6f;

		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("Basic Finisher"), Space]
		private int BasicFinisherCount = 3;
		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("Basic Finisher")]
		private int BasicFinisherChain = 3;
		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("Basic Finisher")]
		private MissileSpawnBehavior BasicFinisherBehavior = MissileSpawnBehavior.SpreadRandom;
		[SerializeField, HideIf("@BasicFinisherBehavior == MissileSpawnBehavior.Direct || !HasBasicFinisher"), FoldoutGroup("Basic Finisher"), Unit(Units.Degree)]
		private float BasicFinisherSpread = 45f;
		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("Basic Finisher"), Unit(Units.MetersPerSecond)]
		private float BasicFinisherKnockbackSpeed = 14f;

		[SerializeField, ShowIf(nameof(HasBasicFinisher)), FoldoutGroup("Basic Finisher"), Space]
		[Tooltip("This can be null to use the basic normal missile.")]
		private Missile BasicFinisherMissileOverride;

		[SerializeField, FoldoutGroup("Basic Finisher"), ShowIf(nameof(HasBasicFinisher)), Space]
		private float BasicFinisherEnergyUse = 4f;
		[SerializeField, FoldoutGroup("Basic Finisher"), ShowIf(nameof(HasBasicFinisher)), Space]
		private float BasicFinisherManaConsume = 12f;

		[Space, SerializeField, FoldoutGroup("Special")]
		private bool HasSpecial = true;
		[SerializeField, Required, FoldoutGroup("Special"), ShowIf(nameof(HasSpecial))]
		private Missile SpecialMissile;
		[SerializeField, Required, FoldoutGroup("Special"), ShowIf(nameof(HasSpecial))]
		private bool RequireFullChargeForSpecial;

		[SerializeField, FoldoutGroup("Special"), ShowIf(nameof(HasSpecial)), Unit(Units.Second), Space]
		private float ChargingSparkInterval = 0.45f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special"), Unit(Units.Second)]
		private float SpecialCooldown = 1f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special"), Unit(Units.Second)]
		private float SpecialChargeTime = 1f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special"), Unit(Units.MetersPerSecond)]
		private float SpecialKnockbackSpeed = 10f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float ChargingMoveSpeedFactor = 0.5f;

		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special"), Space]
		private int SpecialCount = 1;
		[SerializeField, ShowIf("@SpecialCount > 1 && HasSpecial"), FoldoutGroup("Special"), Unit(Units.Second)]
		private float SpecialInterval = 0f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private MissileSpawnBehavior SpecialBehavior = MissileSpawnBehavior.Direct;
		[SerializeField, HideIf("@SpecialBehavior == MissileSpawnBehavior.Direct || !HasSpecial"), FoldoutGroup("Special"), Unit(Units.Degree)]
		private float SpecialSpread = 0f;

		[SerializeField, FoldoutGroup("Special"), ShowIf(nameof(HasSpecial)), Space]
		private float SpecialEnergyUse = 8f;
		[SerializeField, FoldoutGroup("Special"), ShowIf(nameof(HasSpecial))]
		private float SpecialManaConsume = 34f;

		private float _largeMissileTime;
		private int _castChainCount;
		private float _chainTimeoutTime;
		private bool _isMovePenaltyApplied;

		private bool _isCharging;

		public void Update()
		{
			if (IsBasicHeld && CanCastExcludingCost)
			{
				OnBasicDown();
			}
		}

		public void FixedUpdate()
		{
			if (Caster == null)
				return;

			// Were charging, not anymore.
			if (_isCharging && !IsSpecialHeld)
			{
				_isCharging = false;
				Caster.Movement.RemoveSpeedModifier(this);
			}

			// Reset casting chain.
			if (_castChainCount < BasicFinisherCount && _castChainCount > 0 && Time.time > _chainTimeoutTime)
			{
				_castChainCount = 0;
				Caster.SetCooldown(BasicCooldown);
			}

			// Finished charging to max charge.
			if (_isMovePenaltyApplied && Time.time > _largeMissileTime && HasSpecial && IsSpecialHeld && _isCharging)
			{
				_isMovePenaltyApplied = false;
				Caster.Movement.RemoveSpeedModifier(this);

				Audio.Play(FullChargeSound);
			}

			// Charging staff spark.
			if (_isCharging)
			{
				Cooldown.Call(this, ChargingSparkInterval, Caster.Spark);
			}

			// Handle crosshair charge percent.
			if (_isCharging)
			{
				float delta = _largeMissileTime - Time.time;
				Caster.SetCharge(Mathf.Min(1f - (delta / SpecialChargeTime), 1f));
			}
			else
			{
				Caster.SetCharge(0f);
			}
		}

		public override void OnBasicDown()
		{
			if (!CanCastExcludingCost || IsSpecialHeld || !CanAffordCost(BasicManaConsume))
				return;

			_castChainCount++;
			Caster.Spark();

			// Finisher cast.
			if (_castChainCount >= BasicFinisherChain && HasBasicFinisher)
			{
				Caster.SetCooldown(BasicFinisherCooldown);
				_castChainCount = 0;

				var missile = BasicFinisherMissileOverride != null ? BasicFinisherMissileOverride : BasicMissile;

				MissileManager.Instance.SpawnMissile(Caster.gameObject, missile, Head.position, GetDirToCrosshair(),
					BasicFinisherCount, BasicFinisherBehavior, BasicFinisherSpread);
				Caster.Movement.Knockback(-GetDirToCrosshair(), BasicFinisherKnockbackSpeed);

				Audio.Play(BasicFinisherCastSound, Head.position);
				ConsumeEnergy(BasicFinisherEnergyUse);

				ConsumeMana(BasicFinisherManaConsume);
			}
			// Normal cast.
			else
			{
				Caster.SetCooldown(BasicCooldown);
				MissileManager.Instance.SpawnMissile(Caster.gameObject, BasicMissile, Head.position, GetDirToCrosshair(),
					BasicCount, BasicInterval, BasicBehavior, BasicSpread);

				_chainTimeoutTime = Time.time + ChainWindowDuration + BasicCooldown;
				Caster.Movement.Knockback(-GetDirToCrosshair(), BasicKnockbackSpeed);

				Audio.Play(BasicCastSound, Head.position);
				ConsumeEnergy(BasicEnergyUse);

				ConsumeMana(BasicManaConsume);
			}
		}

		public override void OnSpecialDown()
		{
			if (!HasSpecial || !CanCastExcludingCost || !CanAffordCost(SpecialManaConsume))
				return;

			_isCharging = true;

			Caster.Movement.CanDash = false;

			Caster.SetCooldown(SpecialCooldown);
			_largeMissileTime = Time.time + SpecialChargeTime;

			Caster.Movement.ApplySpeedModifier(this, ChargingMoveSpeedFactor);
			_isMovePenaltyApplied = true;
			_castChainCount = 0;

			CanRegenMana = false;
		}

		public override void OnSpecialUp()
		{
			CanRegenMana = true;

			Caster.Movement.RemoveSpeedModifier(this);
			Caster.Movement.CanDash = true;

			if (!HasSpecial || !CanCastExcludingCost || !CanAffordCost(SpecialManaConsume) || !_isCharging)
			{
				_isCharging = false;
				return;
			}

			// Mustn't set to false because the if statement above must check it first.
			_isCharging = false;

			Caster.Spark();
			bool enoughCharge = Time.time > _largeMissileTime;

			if (!enoughCharge && RequireFullChargeForSpecial)
			{
				return;
			}

			var prefab = enoughCharge ? SpecialMissile : BasicMissile;
			MissileManager.Instance.SpawnMissile(Caster.gameObject, prefab, Head.position, GetDirToCrosshair(),
				SpecialCount, SpecialInterval, SpecialBehavior, SpecialSpread);

			Caster.Movement.Knockback(-GetDirToCrosshair(), SpecialKnockbackSpeed);
			Audio.Play(enoughCharge ? SpecialCastBigSound : SpecialCastLittleSound, Head.position);

			if (enoughCharge)
			{
				ConsumeEnergy(SpecialEnergyUse);
				ConsumeMana(SpecialManaConsume);
			}
			else
			{
				ConsumeEnergy(BasicFinisherEnergyUse);
				ConsumeMana(BasicFinisherManaConsume);
			}
		}

		private Vector2 GetDirToCrosshair()
		{
			return CrosshairManager.Instance.DirectionToCrosshair(Head.position);
		}
	}
}
