using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class CombustionStaff : Staff
	{
		[SerializeField, Unit(Units.Second)]
		private float CastCooldown = 0.1f;
		[SerializeField, Unit(Units.Second)]
		private float ChargeRate = 1f;
		[SerializeField, Unit(Units.Second)]
		private float MaxCharge = 4f;
		[SerializeField]
		private float MinChargeThreshold = 1f;

		[Space, SerializeField]
		private float MaxDamage = 51f;
		[SerializeField]
		private AnimationCurve DamageChargeFactor;
		[SerializeField]
		private float DamageRadius = 1f;

		[Space, SerializeField]
		private float MaxEnergyUse = 5f;
		[SerializeField]
		private AnimationCurve EnergyUseChargeFactor;
		[SerializeField]
		private float FriendlyKnockbackSpeed = 18f;
		[SerializeField]
		private float MaxManaConsume = 20f;

		[Space, SerializeField]
		private float BaseSparkCooldown = 0.3f;
		[SerializeField]
		private GameObject CombustionVFX;
		[SerializeField]
		private float VFXLifespan = 3f;
		[SerializeField]
		private int MinFireVFXCount = 32, MaxFireVFXCount = 128;
		[SerializeField]
		private int MinSmokeVFXCount = 16, MaxSmokeVFXCount = 64;

		[SerializeField, FoldoutGroup("SFX")]
		private EventReference SmallExplosionSound, MediumExplosionSound, LargeExplosionSound, FullChargeSound;

		private bool _isCharging;
		private float _charge;

		private bool _isFullCharge;

		public void FixedUpdate()
		{
			if (Caster == null)
				return;

			if (_isCharging && Caster != null)
			{
				_charge += Time.fixedDeltaTime * ChargeRate;
				_charge = Mathf.Min(_charge, MaxCharge);

				float cooldown = BaseSparkCooldown;
				float percent = _charge / MaxCharge;
				if (percent > 0.3f) cooldown *= 0.4f;
				else if (percent > 0.8f) cooldown *= 0.1f;
				Cooldown.Call(this, BaseSparkCooldown, Caster.Spark);

				if (percent > 0.99f && !_isFullCharge)
				{
					_isFullCharge = true;
					Audio.Play(FullChargeSound);
				}
			}
			else
			{
				_isFullCharge = false;
			}

			CanRegenMana = !_isCharging;

			if (_isCharging)
				Caster.SetCharge(_charge / MaxCharge);
			else
				Caster.SetCharge(0f);
		}

		public override string ToString()
		{
			return Name;
		}

		public override void OnBasicDown()
		{
			if (CanCastExcludingCost)
			{
				_isCharging = true;

				Caster.Movement.CanDash = false;
				Caster.Movement.ApplySpeedModifier(this, 0.85f);
			}
		}

		public override void OnBasicUp()
		{
			Caster.Movement.CanDash = true;
			Caster.Movement.RemoveSpeedModifier(this);

			if (!_isCharging)
				return;

			Caster.SetCooldown(CastCooldown);
			_isCharging = false;

			if (_charge >= MinChargeThreshold)
			{
				Vector2 pos = CrosshairManager.Instance.Position;
				float chargePercent = (_charge - MinChargeThreshold) / (MaxCharge - MinChargeThreshold);

				var vfx = CombustionVFX.Clone(pos);
				vfx.Destroy(VFXLifespan);

				var fx = vfx.GetComponentsInChildren<VisualEffect>();

				fx[0].SetInt("Count", (int)Mathf.Lerp(MinFireVFXCount, MaxFireVFXCount, chargePercent));
				fx[0].SetFloat("SpeedFactor", Mathf.Lerp(0.5f, 2f, chargePercent));

				fx[1].SetInt("Count", (int)Mathf.Lerp(MinSmokeVFXCount, MaxSmokeVFXCount, chargePercent));
				fx[1].SetFloat("SpeedFactor", Mathf.Lerp(0.5f, 2f, chargePercent));

				fx[2].SetInt("SpawnCount", (int)Mathf.Lerp(MinSmokeVFXCount, MaxSmokeVFXCount, chargePercent));
				fx[2].SetFloat("SpeedFactor", Mathf.Lerp(0.5f, 2f, chargePercent));

				foreach (var f in fx)
				{
					f.Play();
				}

				EventReference sound;
				if (chargePercent > 0.8f)
					sound = LargeExplosionSound;
				else if (chargePercent > 0.3f)
					sound = MediumExplosionSound;
				else
					sound = SmallExplosionSound;

				Audio.Play(sound, pos);

				ConsumeEnergy(MaxEnergyUse * EnergyUseChargeFactor.Evaluate(chargePercent));
				ConsumeMana(MaxManaConsume * chargePercent);

				_charge = 0f;

				// Overlap colliders.
				var colliders = Physics2D.OverlapCircleAll(pos, DamageRadius);

				// No damage is dealt if we hit a missile blocker.
				foreach (var collider in colliders)
				{
					if (collider.CompareTag("MissileBlocker"))
					{
						return;
					}
				}

				// Actual damage application.
				foreach (var collider in colliders)
				{
					if (collider.TryGetComponent(out IDamageable damageable))
					{
						if (damageable.Team != Team.Player)
						{
							float dmg = DamageChargeFactor.Evaluate(chargePercent);
							dmg *= MaxDamage;

							damageable.TakeDamage(dmg, pos.DirectionTo(collider.transform.position), Team.Player, Caster.gameObject);
						}
						else if (collider.TryGetComponent(out Locomotion locomotion))
						{
							locomotion.Knockback(pos.DirectionTo(collider.transform.position), FriendlyKnockbackSpeed);
						}
					}
					else if (collider.TryGetComponent(out SteamGenerator gen))
					{
						gen.Generate();
					}
				}
			}
		}
	}
}
