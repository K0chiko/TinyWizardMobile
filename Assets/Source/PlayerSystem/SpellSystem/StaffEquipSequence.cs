using DG.Tweening;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem.SpellSystem
{
	public sealed class StaffEquipSequence
	{
		private readonly Component _owner;
		private readonly Light2D _light;
		private readonly VisualEffect _equipVfx, _energyVfx;
		private readonly EventReference _sound;
		private readonly float _energyTransferRate;

		public StaffEquipSequence(Component owner, Light2D light, VisualEffect equipVfx, VisualEffect energyVfx, EventReference sound, float energyTransferRate)
		{
			_owner = owner;
			_light = light;
			_equipVfx = equipVfx;
			_energyVfx = energyVfx;
			_sound = sound;
			_energyTransferRate = energyTransferRate;
		}

		public async Awaitable Play(Staff currentStaff, Staff targetStaff, Action<Staff> onTargetChosen)
		{
			float energyToTransfer = currentStaff.Energy * _energyTransferRate;

			InputManager.Instance.DisableInput();
			var hp = _owner.GetComponent<Health>();
			hp.BlockDamage(this);

			_light.intensity = 0f;
			_light.enabled = true;
			_light.DOFade(1f, 0.5f);

			_owner.GetComponent<Animator>().SetTrigger("EquipSequence");

			onTargetChosen(targetStaff);
			DOTween.To(() => targetStaff.Energy, x => targetStaff.SetEnergy(x), targetStaff.Energy + energyToTransfer, 4f)
				.SetEase(Ease.OutCubic);

			Transform oldStaffHead = currentStaff.Head;
			Transform newStaffHead = targetStaff.Head;

			targetStaff.transform.DORotate(Vector3.zero, 3f);
			var tween = targetStaff.transform.DOMove(_owner.transform.position + (Vector3.up * 3f), 3f)
				.SetEase(Ease.OutCubic);

			tween.onUpdate += () =>
			{
				UpdateEnergyVfx(oldStaffHead, newStaffHead);
			};

			_energyVfx.SetGradient("Color", currentStaff.SparkGradient);
			_energyVfx.Play();

			await Wait.Seconds(4f);

			_equipVfx.Play();
			Audio.Play(_sound, _owner.transform.position);

			await _light.DOFade(0f, 0.2f).AsyncWaitForCompletion();
			_light.enabled = false;
			_energyVfx.Stop();

			InputManager.Instance.EnableInput();
			hp.UnblockDamage(this);
		}

		private void UpdateEnergyVfx(Transform oldStaffHead, Transform newStaffHead)
		{
			_energyVfx.SetVector2("Start", oldStaffHead.position);
			_energyVfx.SetVector2("End", newStaffHead.position);

			Vector2 startBias = Vector2.Lerp(oldStaffHead.position, newStaffHead.position, 0.15f);
			Vector2 endBias = Vector2.Lerp(oldStaffHead.position, newStaffHead.position, 0.85f);

			Vector2 diff = newStaffHead.position - oldStaffHead.position;
			Vector2 perp = new(-diff.x, diff.y);

			startBias += perp;
			endBias += perp;

			_energyVfx.SetVector2("StartBias", startBias);
			_energyVfx.SetVector2("EndBias", endBias);
		}
	}
}
