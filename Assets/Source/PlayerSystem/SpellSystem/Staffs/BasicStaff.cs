using FMODUnity;
using Quinn.MissileSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class BasicStaff : Staff
	{
		[SerializeField]
		private EventReference BasicCastSound;
		[SerializeField]
		private EventReference BasicFinisherCastSound;
		[SerializeField]
		private EventReference SpecialCastLittleSound, SpecialCastBigSound, FullChargeSound;

		[SerializeField]
		private Missile BasicMissile;
		[SerializeField]
		private float BasicCooldown = 0.3f;
		[SerializeField]
		private float BasicKnockbackSpeed = 10f;

		[SerializeField, Space]
		private MissileSpawnBehavior BasicBehavior = MissileSpawnBehavior.Direct;
		[SerializeField]
		private float BasicSpread = 0f;
		[SerializeField]
		private int BasicCount = 1;
		[SerializeField]
		private float BasicInterval = 0f;
		[SerializeField]
		private float ChainWindowDuration = 0.4f;

		[SerializeField, Space]
		private float BasicEnergyUse = 2f;
		[SerializeField]
		private float BasicManaConsume = 4f;

		[Space, SerializeField]
		private bool HasBasicFinisher = true;
		[SerializeField]
		private float BasicFinisherCooldown = 0.6f;

		[SerializeField, Space]
		private int BasicFinisherCount = 3;
		[SerializeField]
		private int BasicFinisherChain = 3;
		[SerializeField]
		private MissileSpawnBehavior BasicFinisherBehavior = MissileSpawnBehavior.SpreadRandom;
		[SerializeField]
		private float BasicFinisherSpread = 45f;
		[SerializeField]
		private float BasicFinisherKnockbackSpeed = 14f;

		[SerializeField, Space]
		[Tooltip("This can be null to use the basic normal missile.")]
		private Missile BasicFinisherMissileOverride;

		[SerializeField, Space]
		private float BasicFinisherEnergyUse = 4f;
		[SerializeField, Space]
		private float BasicFinisherManaConsume = 12f;

		[Space, SerializeField]
		private bool HasSpecial = true;
		[SerializeField]
		private Missile SpecialMissile;
		[SerializeField]
		private bool RequireFullChargeForSpecial;

		[SerializeField, Space]
		private float ChargingSparkInterval = 0.45f;
		[SerializeField]
		private float SpecialCooldown = 1f;
		[SerializeField]
		private float SpecialChargeTime = 1f;
		[SerializeField]
		private float SpecialKnockbackSpeed = 10f;
		[SerializeField]
		private float ChargingMoveSpeedFactor = 0.5f;

		[SerializeField, Space]
		private int SpecialCount = 1;
		[SerializeField]
		private float SpecialInterval = 0f;
		[SerializeField]
		private MissileSpawnBehavior SpecialBehavior = MissileSpawnBehavior.Direct;
		[SerializeField]
		private float SpecialSpread = 0f;

		[SerializeField, Space]
		private float SpecialEnergyUse = 8f;
		[SerializeField]
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
