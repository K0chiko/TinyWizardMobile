using DG.Tweening;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class VignetteUI : MonoBehaviour
	{
		[SerializeField]
		private float HurtDuration = 1.5f;
		[SerializeField]
		private float FadeInDuration = 0.2f;
		[SerializeField]
		private float FadeOutDuration = 1f;

		[SerializeField, Required]
		private Image HurtVignette;

		public void Start()
		{
			PlayerManager.Instance.Player.GetComponent<Health>().OnDamagedExpanded += OnHurt;
		}

		private void OnHurt(DamageInfo info)
		{
			HurtVignette.DOKill();

			HurtVignette.DOFade(1f, FadeInDuration)
				.onComplete += () =>
				{
					DOVirtual.DelayedCall(HurtDuration, () =>
					{
						HurtVignette.DOFade(0f, FadeOutDuration);
					});
				};
		}
	}
}
