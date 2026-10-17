using DG.Tweening;
using System;
using UnityEngine;

namespace Quinn
{
	public sealed class HurtFeedback
	{
		private readonly UnityEngine.Object _owner;
		private readonly SpriteRenderer[] _renderers;
		private readonly float _flashInDuration, _flashHoldDuration, _flashOutDuration;
		private readonly Ease _flashInEase, _flashOutEase;
		private readonly int _blinkCount;
		private readonly float _blinkInterval;
		private readonly Func<bool> _shouldBlink;

		public HurtFeedback(
			UnityEngine.Object owner,
			SpriteRenderer[] renderers,
			float flashInDuration, float flashHoldDuration, float flashOutDuration,
			Ease flashInEase, Ease flashOutEase,
			int blinkCount, float blinkInterval,
			Func<bool> shouldBlink)
		{
			_owner = owner;
			_renderers = renderers;
			_flashInDuration = flashInDuration;
			_flashHoldDuration = flashHoldDuration;
			_flashOutDuration = flashOutDuration;
			_flashInEase = flashInEase;
			_flashOutEase = flashOutEase;
			_blinkCount = blinkCount;
			_blinkInterval = blinkInterval;
			_shouldBlink = shouldBlink;
		}

		public async void Play()
		{
			foreach (var renderer in _renderers)
			{
				if (renderer == null)
				{
					Debug.LogError($"Health damaged FX found a null sprite renderered! Make sure '{_owner.name}' has no missing references on its Health component.");
					continue;
				}

				renderer.enabled = true;
				renderer.material.SetFloat("_Flash", 0f);
			}

			await Flash();

			if (_shouldBlink())
			{
				Blink();
			}
		}

		private async Awaitable Flash()
		{
			float Get()
			{
				if (_renderers.Length == 0)
					return 0f;

				var renderer = _renderers[0];

				if (renderer == null)
					return 0f;

				return renderer.material.GetFloat("_Flash");
			}
			void Set(float value)
			{
				if (_owner == null)
					return;

				foreach (var renderer in _renderers)
				{
					if (renderer == null)
						return;

					renderer.material.SetFloat("_Flash", value);
				}
			}

			if (_renderers.Length > 0 && _owner != null)
			{
				await DOTween.To(Get, Set, 1f, _flashInDuration)
					.SetEase(_flashInEase)
					.AsyncWaitForCompletion();

				await Awaitable.WaitForSecondsAsync(_flashHoldDuration);

				await DOTween.To(Get, Set, 0f, _flashOutDuration)
					.SetEase(_flashOutEase)
					.AsyncWaitForCompletion();
			}
		}

		private async void Blink()
		{
			for (int i = 0; i < _blinkCount; i++)
			{
				await Awaitable.WaitForSecondsAsync(_blinkInterval);

				foreach (var renderer in _renderers)
				{
					renderer.enabled = false;
				}

				await Awaitable.WaitForSecondsAsync(_blinkInterval);

				foreach (var renderer in _renderers)
				{
					renderer.enabled = true;
				}
			}
		}
	}
}
