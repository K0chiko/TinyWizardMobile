using DG.Tweening;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class GameOverUI : MonoBehaviour
	{
		[SerializeField, Required]
		private TextMeshProUGUI Title;
		[SerializeField, Required]
		private Image Spacer;
		[SerializeField, Required]
		private Image RetryButton, QuitButton;

		public void Start()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			Title.alpha = 0f;
			Title.DOFade(1f, 2f).SetEase(Ease.OutCubic);

			var col = RetryButton.color;
			col.a = 0f;
			RetryButton.color = col;

			col = QuitButton.color;
			col.a = 0f;
			QuitButton.color = col;

			col = Spacer.color;
			col.a = 0f;
			Spacer.color = col;

			Spacer.DOFade(2f, 1f);

			var pos = Spacer.transform.position;
			pos.y -= 0.5f;
			Spacer.transform.position = pos;

			Spacer.transform.DOMoveY(pos.y + 0.5f, 0.2f).SetEase(Ease.OutBack);

			AnimateButton(RetryButton);
			AnimateButton(QuitButton);
		}

		public void OnDestroy()
		{
			Title.DOKill();
			Spacer.DOKill();

			RetryButton.DOKill();
			QuitButton.DOKill();

			RetryButton.rectTransform.DOKill();
			QuitButton.rectTransform.DOKill();
		}

		public async void Retry_Button()
		{
			await Wait.Seconds(0.1f);

			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = false;

			Destroy(gameObject);
			PlayerManager.Instance.RespawnSequence();
		}

		public async void Quit_Button()
		{
			await Wait.Seconds(0.1f);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		private async void AnimateButton(Image button)
		{
			var btn = button.GetComponent<Button>();
			btn.interactable = false;

			button.DOFade(1f, 1f).SetEase(Ease.InCubic);

			float y = button.transform.position.y;

			var vec = button.transform.position;
			vec.y -= 5f;
			button.transform.position = vec;

			await button.rectTransform.DOMoveY(y, 0.5f)
				.SetEase(Ease.OutBack)
				.AsyncWaitForCompletion();

			if (btn != null)
				btn.interactable = true;
		}
	}
}
