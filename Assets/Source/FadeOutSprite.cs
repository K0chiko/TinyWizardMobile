using DG.Tweening;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class FadeOutSprite : MonoBehaviour
	{
		[SerializeField]
		private bool IsManual;
		[SerializeField]
		private bool DestroyOnFade = true;

		[Space, SerializeField]
		private float FadeDuration = 2f;
		[SerializeField]
		private Ease FadeEase = Ease.InCubic;

		public void Awake()
		{
			if (!IsManual)
			{
				Fade();
			}
		}

		public async void Fade()
		{
			await GetComponent<SpriteRenderer>().DOFade(0f, FadeDuration)
				.SetEase(FadeEase)
				.AsyncWaitForCompletion();

			if (DestroyOnFade)
			{
				Destroy(gameObject);
			}
		}
	}
}
