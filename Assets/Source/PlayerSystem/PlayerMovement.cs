using FMODUnity;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Health))]
	public class PlayerMovement : Locomotion
	{
		[SerializeField]
		private float MoveSpeed = 6f;

		[SerializeField, BoxGroup("Vortex")]
		private float VortexMaxSpeed = 6f;
		[SerializeField, BoxGroup("Vortex")]
		private float VortexMaxRadius = 24f;

		[SerializeField, BoxGroup("Dash")]
		private float DashSpeed = 12f;
		[SerializeField, BoxGroup("Dash")]
		private float DashDistance = 4f;
		[SerializeField, BoxGroup("Dash")]
		private float DashCooldown = 0.2f;
		[SerializeField, BoxGroup("Dash")]
		private EventReference DashSound;

		[Space, SerializeField, Required]
		private VisualEffect DashTrail;

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
			float scale = Rigidbody.linearVelocity.magnitude / MoveSpeed;
			if (IsDashing) scale = 1f;
			_animator.SetFloat("SpeedScale", scale);

			_animator.SetBool("IsDashing", IsDashing);
			DashTrail.SetBool("Enabled", IsDashing);
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
				vel += DashDirection * DashSpeed;

				if (Time.time > _dashEndTime)
				{
					IsDashing = false;
					OnDashStop();
				}
			}
			else
			{
				float moveSpeed = _speedOverride ?? MoveSpeed;
				vel += moveSpeed * moveDir;

				if (_vortexOrigin != null)
				{
					float dstToVortex = transform.position.DistanceTo(_vortexOrigin.position);
					Vector2 dirToVortex = transform.position.DirectionTo(_vortexOrigin.position);

					float t = Mathf.Clamp01(dstToVortex / VortexMaxRadius);
					float vortexSpeed = Mathf.Lerp(VortexMaxSpeed, 0f, t);

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

				Audio.Play(DashSound, transform.position);
				float dashDur = DashDistance / DashSpeed;

				_dashEndTime = Time.time + dashDur;
				_nextDashTime = Time.time + DashCooldown;
			}
		}

		private void OnDashStop()
		{
			_health.UnblockDamage(this);
		}

		private void OnStaffEquiped(Staff staff)
		{
			DashTrail.SetGradient("Color", staff.SparkGradient);
		}
	}
}
