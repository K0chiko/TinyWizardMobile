using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Quinn
{
	public static class DOTweenExtensions
	{
		public static TweenerCore<float, float, FloatOptions> DOAnimateFloat(this Material material, string propertyName, float endValue, float duration)
		{
			var tween = DOTween.To(() => material.GetFloat(propertyName), x => material.SetFloat(propertyName, x), endValue, duration);
			tween.target = material;
			return tween;
		}

		public static TweenerCore<float, float, FloatOptions> DOFade(this Light2D light, float endValue, float duration)
		{
			var tween = DOTween.To(() => light.intensity, x => light.intensity = x, endValue, duration);
			tween.target = light;
			return tween;
		}
	}
}
