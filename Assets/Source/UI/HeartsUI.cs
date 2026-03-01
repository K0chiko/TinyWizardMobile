using DG.Tweening;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HeartsUI : MonoBehaviour
	{
		[SerializeField, Required]
		private GameObject HeartPrefab;
		[SerializeField, Required]
		private Sprite FullHeart, HalfHeart, EmptyHeart;

		[SerializeField, BoxGroup("Last Heart Shake")]
		private float LastHeartAmplitude = 5f, LastHeartFrequency = 50f;

		[SerializeField, BoxGroup("HP Change Punch")]
		private float PunchScale = 1.1f, PunchDur = 0.2f;
		[SerializeField, BoxGroup("HP Change Punch")]
		private Ease PunchBigEase = Ease.Linear, PunchSmallEase = Ease.Linear;

		[SerializeField, BoxGroup("SFX")]
		private EventReference GainHeartSound;

		public static HeartsUI Instance { get; private set; }

		private readonly List<Image> _hearts = new();

		public void Awake()
		{
			Instance = this;
		}

		public void Start()
		{
			PlayerManager.Instance.OnPlayerHealthChange += OnHealthChange;
			PlayerManager.Instance.OnPlayerMaxHealthChange += OnMaxHealthChange;

			transform.DestroyChildren();
			ReconstructHearts();
		}

		public void Update()
		{
			if (PlayerManager.Instance.Health != null && PlayerManager.Instance.Health.Current == 1f)
			{
				transform.localPosition = new Vector3()
				{
					x = Mathf.Sin(Time.time * LastHeartFrequency),
					y = Mathf.Cos((Time.time + Random.value) * LastHeartFrequency)
				} * LastHeartAmplitude;
			}
			else
			{
				transform.localPosition = Vector3.zero;
			}
		}

		public void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.OnPlayerHealthChange -= OnHealthChange;
				PlayerManager.Instance.OnPlayerMaxHealthChange -= OnMaxHealthChange;
			}
		}

		public void Regenerate()
		{
			ReconstructHearts();
			UpdateHearts();
		}

		private async void OnHealthChange(float delta)
		{
			if (delta != 0f)
			{
				if (delta > 0f)
				{
					Audio.Play(GainHeartSound);
				}

				UpdateHearts(isHealing: delta > 0f);

				await transform.parent.DOScale(PunchScale, PunchDur / 2f)
					.SetEase(PunchBigEase)
					.AsyncWaitForCompletion();

				transform.parent.DOScale(1, PunchDur / 2f)
					.SetEase(PunchSmallEase);
			}
		}

		private void OnMaxHealthChange()
		{
			ReconstructHearts();
			Regenerate();
		}

		private void ReconstructHearts()
		{
			foreach (var heart in _hearts.ToArray())
			{
				Destroy(heart.gameObject);
			}

			_hearts.Clear();

			int max = Mathf.RoundToInt(PlayerManager.Instance.Health.Max);

			for (int i = 0; i < max; i++)
			{
				var heart = HeartPrefab.Clone(transform);
				_hearts.Add(heart.GetComponent<Image>());
			}
		}

		private void UpdateHearts(bool isHealing = false)
		{
			int current = Mathf.RoundToInt(PlayerManager.Instance.Health.Current);

			for (int i = 0; i < _hearts.Count; i++)
			{
				var child = _hearts[i];

				bool isNotEmpty = i < current;
				child.sprite = isNotEmpty ? FullHeart : EmptyHeart;

				if (isNotEmpty && PlayerManager.Instance.Health.IsHalfHeart)
				{
					child.sprite = HalfHeart;
				}

				if (isNotEmpty && isHealing)
				{
					PunchHeart(child.transform, i);
				}
			}
		}

		private async void PunchHeart(Transform heart, int index)
		{
			await Wait.Seconds(index * 0.1f);

			await heart.DOScale(2f, 0.1f).AsyncWaitForCompletion();
			heart.DOScale(1f, 0.1f);
		}
	}
}
