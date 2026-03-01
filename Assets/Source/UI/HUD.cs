using DG.Tweening;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HUD : MonoBehaviour
	{
		[SerializeField, Required]
		private CanvasGroup Group;
		[SerializeField]
		private float FadeDuration = 1f;

		[SerializeField, Required, Space]
		private Image LowHPVignette;
		[SerializeField]
		private float SmoothTime = 0.2f;
		[SerializeField]
		private float ScaleFrequency = 0.5f;
		[SerializeField]
		private float ScaleAmplitude = 0.2f;

		[SerializeField, Required, Space]
		private TextMeshProUGUI Dialogue;
		[SerializeField]
		private float WriteInterval = 0.01f;
		[SerializeField]
		private EventReference WriteSound;

		[Space, SerializeField, Required]
		private TextMeshProUGUI[] HelpText;

		public static HUD Instance { get; private set; }

		public float Alpha => Group.alpha;

		private CancellationTokenSource _cancelDialogue = new();

		private float _alphaVel;
		private float _defaultScale;

		public void Awake()
		{
			Instance = this;

			Dialogue.alpha = 0f;
			_defaultScale = LowHPVignette.transform.localScale.y;

			foreach (var text in HelpText)
			{
				text.alpha = 0f;
			}
		}

		public void Update()
		{
			var color = LowHPVignette.color;

			float targetAlpha = 0f;
			if (PlayerManager.Instance.IsDead || PlayerManager.Instance.Health.Current <= 1.55f)
			{
				targetAlpha = 1f;
			}
			else if (PlayerManager.Instance.Health.Current < 2.55f)
			{
				targetAlpha = 0.3f;
			}

			color.a = Mathf.SmoothDamp(LowHPVignette.color.a, targetAlpha, ref _alphaVel, SmoothTime);
			LowHPVignette.color = color;

			float scale = Mathf.Sin(Time.time * ScaleFrequency) * ScaleAmplitude;
			scale += _defaultScale;
			LowHPVignette.transform.localScale = new Vector3(scale, scale, 1f);

			if (Input.GetKeyDown(KeyCode.Tab))
			{
				foreach (var text in HelpText)
				{
					text.DOFade(1f, 0.1f);
				}
			}
			else if (Input.GetKeyUp(KeyCode.Tab))
			{
				foreach (var text in HelpText)
				{
					text.DOFade(0f, 0.2f);
				}
			}
		}

		public void OnDestroy()
		{
			Group.DOKill();
			_cancelDialogue.Cancel();
		}

		public void Hide()
		{
			Group.alpha = 0f;
		}

		public void FadeIn()
		{
			Group.DOFade(1f, FadeDuration);
		}

		public async void WriteDialogue(string text)
		{
			_cancelDialogue.Cancel();
			_cancelDialogue = new();

			Dialogue.text = string.Empty;

			if (Dialogue.alpha == 0f) {
				Dialogue.DOFade(1f, 0.1f);
			}

			var builder = new StringBuilder();

			for (int i = 0; i < text.Length; i++)
			{
				if (_cancelDialogue.IsCancellationRequested)
				{
					return;
				}

				builder.Append(text[i]);
				Dialogue.text = builder.ToString();

				Audio.Play(WriteSound);
				await Wait.Seconds(WriteInterval, _cancelDialogue.Token);
			}
		}

		public void HideDialogue()
		{
			Dialogue.DOFade(0f, 0.1f);
		}
	}
}
