using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Quinn.UI
{
	[RequireComponent(typeof(Button))]
	public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField]
		private EventReference HoverSound, ClickSound;

		private Button _button;

		public void Awake()
		{
			_button = GetComponent<Button>();
		}

		public void OnDestroy()
		{
			transform.DOKill();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!_button.interactable)
				return;

			Audio.Play(HoverSound);
			transform.DOScale(1.05f, 0.1f)
				.SetEase(Ease.OutCubic)
				.SetUpdate(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.DOScale(1, 0.2f)
				.SetUpdate(true);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!_button.interactable)
				return;

			Audio.Play(ClickSound);

			transform.DOMoveY(transform.position.y - 0.1f, 0.15f)
				.SetEase(Ease.OutBack)
				.SetUpdate(true);
		}
	}
}
