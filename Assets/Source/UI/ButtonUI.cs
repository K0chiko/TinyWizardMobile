using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Quinn.UI
{
	public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField]
		private EventReference HoverSound, ClickSound;

		public void OnDestroy()
		{
			transform.DOKill();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Audio.Play(HoverSound);
			transform.DOScale(1.05f, 0.1f).SetEase(Ease.OutCubic);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.DOScale(1, 0.2f);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Audio.Play(ClickSound);

			transform.DOMoveY(transform.position.y - 0.1f, 0.15f)
				.SetEase(Ease.OutBack);
		}
	}
}
