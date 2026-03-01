using DG.Tweening;
using Quinn.PlayerSystem;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn
{
	public class CameraManager : MonoBehaviour
	{
		public static CameraManager Instance { get; private set; }

		[SerializeField]
		private float FadeToBlackDuration = 0.3f;
		[SerializeField]
		private float FadeFromBlackDuration = 0.2f;
		[SerializeField]
		private Ease FadeToBlackEase = Ease.InCubic;
		[SerializeField]
		private Ease FadeFromBlackEase = Ease.OutCubic;
		[SerializeField]
		private float FadeToBlackOnDeathDuration = 2f;

		[Space, SerializeField]
		private float PlayerToCursorBias = 0.2f;
		[SerializeField]
		private float CameraTargetSmoothTime = 0.2f;

		public Transform CameraTarget => _handle.CameraTarget;

		private CameraHandle _handle;
		private CinemachineCamera _followCam;
		private Image _blackout;

		private Vector2 _camTargetVel;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		public void Update()
		{
			if (PlayerManager.Instance.IsAlive)
			{
				Vector2 playerPos = PlayerManager.Instance.Player.transform.position;
				Vector2 cursorPos = InputManager.Instance.CursorWorldPos;

				Vector2 cameraTarget = Vector2.Lerp(playerPos, cursorPos, PlayerToCursorBias);
				CameraTarget.position = Vector2.SmoothDamp(CameraTarget.position, cameraTarget, ref _camTargetVel, CameraTargetSmoothTime);
			}
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void SetCameraHandle(CameraHandle handle)
		{
			_handle = handle;

			_followCam = _handle.VirtualCamera;
			_blackout = _handle.Blackout;
		}

		public void EnableFollowCamera()
		{
			Debug.Assert(_followCam != null, "Cannot enable follow camera because it is null");
			_followCam.enabled = true;
		}

		public void DisableFollowCamera()
		{
			Debug.Assert(_followCam != null, "Cannot disable follow camera because it is null");
			_followCam.enabled = false;
		}

		public async Awaitable FadeIn()
		{
			Blackout();

			await _blackout.DOFade(0f, FadeFromBlackDuration)
				.SetEase(FadeFromBlackEase)
				.AsyncWaitForCompletion();

			_blackout.enabled = false;
		}

		/// <summary>
		/// Change the UI element used for fading to be fully black and opaque instantly.
		/// </summary>
		public void Blackout()
		{
			_blackout.enabled = true;
			_blackout.color = new Color(0f, 0f, 0f, 1f);
		}

		public async Awaitable FadeOut()
		{
			_blackout.enabled = true;
			_blackout.color = new Color(0f, 0f, 0f, 0f);

			await _blackout.DOFade(1f, FadeToBlackDuration)
				.SetEase(FadeToBlackEase)
				.AsyncWaitForCompletion();
		}

		public async Awaitable DeathFadeOut()
		{
			_blackout.enabled = true;
			_blackout.color = new Color(0f, 0f, 0f, 0f);

			await _blackout.DOFade(1f, FadeToBlackOnDeathDuration)
				.SetEase(FadeToBlackEase)
				.AsyncWaitForCompletion();
		}

		public async Awaitable TransitionAsync(System.Func<bool> canFadeIn)
		{
			await FadeOut();

			while(!canFadeIn())
			{
				await System.Threading.Tasks.Task.Yield();
			}

			_ = FadeIn();
		}
		public async Awaitable TransitionAsync()
		{
			await TransitionAsync(() => true);
		}
	}
}
