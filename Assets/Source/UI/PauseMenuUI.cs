using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class PauseMenuUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Canvas Canvas;
		[SerializeField, Required]
		private CanvasGroup CanvasGroup;
		[SerializeField, Required]
		private GameObject EventSystem;

		[SerializeField, Required]
		private Slider SFXSlider, MusicSlider;

		public static PauseMenuUI Instance { get; private set; }
		public bool IsPaused { get; private set; }

		private Bus _sfx, _music;

		public void Awake()
		{
			Instance = this;
			CanvasGroup.alpha = 0f;

			RuntimeManager.StudioSystem.getBus("bus:/SFX", out _sfx);
			RuntimeManager.StudioSystem.getBus("bus:/Music", out _music);

			_sfx.setVolume(SFXSlider.value);
			_music.setVolume(MusicSlider.value);

			EventSystem.SetActive(false);
			Canvas.enabled = false;
		}

		public void Update()
		{
			if (IsPaused && PlayerManager.Instance.IsDead)
			{
				Unpause();
				return;
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (PlayerManager.Instance.IsAlive)
				{
					IsPaused = !IsPaused;

					if (IsPaused)
					{
						Pause();
					}
					else
					{
						Unpause();
					}
				}
			}

			if (IsPaused)
			{
				_sfx.setVolume(SFXSlider.value);
				_music.setVolume(MusicSlider.value);
			}
		}

		public void Unpause_Button()
		{
			if (IsPaused)
				Unpause();
		}

		public async void Quit_Button()
		{
			if (!IsPaused)
				return;

			Canvas.GetComponentsInChildren<Button>().ForEach(x => x.interactable = false);
			await Wait.Seconds(0.1f);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		private void Pause()
		{
			IsPaused = true;
			Time.timeScale = 0f;

			EventSystem.SetActive(true);

			Canvas.enabled = true;
			CanvasGroup.DOFade(1f, 0.1f).SetUpdate(true);

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			CrosshairManager.Instance.Hide();

			SFXSlider.interactable = true;
			MusicSlider.interactable = true;
		}

		private void Unpause()
		{
			IsPaused = false;

			EventSystem.SetActive(false);

			Time.timeScale = 1f;
			CanvasGroup.DOFade(0f, 0.1f)
				.SetUpdate(true)
				.onComplete += () => Canvas.enabled = false;

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;

			CrosshairManager.Instance.Show();

			SFXSlider.interactable = false;
			MusicSlider.interactable = false;
		}
	}
}
