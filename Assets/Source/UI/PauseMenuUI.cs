using System.Threading.Tasks;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
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
		private Canvas uiMobile;

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
		}

		public void Update()
		{
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
		
		
		public void TogglePause()
		{
			IsPaused = !IsPaused;
			if (IsPaused) Pause();
			else Unpause();
		}
		
		public async void Quit_Button()
		{
			if (!IsPaused)
				return;

			await Task.Delay(200);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif

			Application.Quit();
		}

		private void Pause()
		{
			IsPaused = true;
			Time.timeScale = 0f;

			Canvas.enabled = true;
			uiMobile.enabled = false;
			CanvasGroup.DOFade(1f, 0.1f).SetUpdate(true);

			//Cursor.visible = true;
			//Cursor.lockState = CursorLockMode.None;

			CrosshairManager.Instance.Hide();

			SFXSlider.interactable = true;
			MusicSlider.interactable = true;
		}

		private void Unpause()
		{
			IsPaused = false;

			Time.timeScale = 1f;
			CanvasGroup.DOFade(0f, 0.1f)
				.SetUpdate(true)
				.onComplete += () => Canvas.enabled = false;

			uiMobile.enabled = true;
			
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;

			CrosshairManager.Instance.Show();

			SFXSlider.interactable = false;
			MusicSlider.interactable = false;
		}
	}
}
