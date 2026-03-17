using FMODUnity;
using Quinn.PlayerSystem.SpellSystem;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Health))]
	public class PlayerMovement : Locomotion
	{
		private static readonly int SpeedScale = Animator.StringToHash("SpeedScale");
		private static readonly int Dashing = Animator.StringToHash("IsDashing");

		[FormerlySerializedAs("MoveSpeed")] [SerializeField]
		private float moveSpeed = 6f;

		[FormerlySerializedAs("VortexMaxSpeed")] [SerializeField]
		private float vortexMaxSpeed = 6f;
		[FormerlySerializedAs("VortexMaxRadius")] [SerializeField]
		private float vortexMaxRadius = 24f;

		[FormerlySerializedAs("DashSpeed")] [SerializeField]
		private float dashSpeed = 12f;
		[FormerlySerializedAs("DashDistance")] [SerializeField]
		private float dashDistance = 4f;
		[FormerlySerializedAs("DashCooldown")] [SerializeField]
		private float dashCooldown = 0.2f;
		[FormerlySerializedAs("DashSound")] [SerializeField]
		private EventReference dashSound;

		[FormerlySerializedAs("DashTrail")] [Space, SerializeField]
		private VisualEffect dashTrail;

		public bool IsDashing { get; private set; }
		public bool CanDash { get; set; } = true;
		public Vector2 DashDirection { get; private set; } = Vector2.down;

		private Animator _animator;
		private Health _health;

		private Transform _vortexOrigin;

		private float _nextDashTime;
		private float _dashEndTime;

		private float? _speedOverride;

		protected override void Awake()
		{
			base.Awake();

			_animator = GetComponent<Animator>();
			_health = GetComponent<Health>();

			InputManager.Instance.OnDash += OnDash;
			GetComponent<PlayerCaster>().OnStaffEquipped += OnStaffEquiped;
		}

		public void Update()
		{
			float scale = Rigidbody.linearVelocity.magnitude / moveSpeed;
			if (IsDashing) scale = 1f;
			_animator.SetFloat(SpeedScale, scale);

			_animator.SetBool(Dashing, IsDashing);
			dashTrail.SetBool("Enabled", IsDashing);
		}

		public void OnDestroy()
		{
			if (InputManager.Instance != null)
				InputManager.Instance.OnDash -= OnDash;
		}

		public override Vector2 GetVelocity()
		{
			Vector2 vel = Vector2.zero;
			Vector2 moveDir = InputManager.Instance.MoveDirection;

			if (IsDashing)
			{
				vel += DashDirection * dashSpeed;

				if (Time.time > _dashEndTime)
				{
					IsDashing = false;
					OnDashStop();
				}
			}
			else
			{
				float moveSpeed = _speedOverride ?? this.moveSpeed;
				vel += moveSpeed * moveDir;

				if (_vortexOrigin != null)
				{
					float dstToVortex = transform.position.DistanceTo(_vortexOrigin.position);
					Vector2 dirToVortex = transform.position.DirectionTo(_vortexOrigin.position);

					float t = Mathf.Clamp01(dstToVortex / vortexMaxRadius);
					float vortexSpeed = Mathf.Lerp(vortexMaxSpeed, 0f, t);

					vel += dirToVortex * vortexSpeed;
				}

				if (moveDir.sqrMagnitude > 0f)
				{
					DashDirection = moveDir;
				}
			}

			return vel;
		}

		public void SetVortexOrigin(Transform origin)
		{
			Debug.Assert(origin != null);
			_vortexOrigin = origin;
		}

		public void ClearVortexOrigin()
		{
			_vortexOrigin = null;
		}

		public void SetSpeedOverride(float speed)
		{
			_speedOverride = speed;
		}

		public void ClearSpeedOverride()
		{
			_speedOverride = null;
		}

		private void OnDash()
		{
			if (CanDash && !IsDashing && Time.time > _nextDashTime)
			{
				IsDashing = true;
				_health.BlockDamage(this);

				Audio.Play(dashSound, transform.position);
				float dashDur = dashDistance / dashSpeed;

				_dashEndTime = Time.time + dashDur;
				_nextDashTime = Time.time + dashCooldown;
			}
		}

		private void OnDashStop()
		{
			_health.UnblockDamage(this);
		}

		private void OnStaffEquiped(Staff staff)
		{
			dashTrail.SetGradient("Color", staff.SparkGradient);
		}
	}
}
