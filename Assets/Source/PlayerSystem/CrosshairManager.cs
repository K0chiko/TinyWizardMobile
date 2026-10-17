using FMODUnity;
using Quinn.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quinn.PlayerSystem
{
	public class CrosshairManager : MonoBehaviour
	{
		public static CrosshairManager Instance { get; private set; }

		[SerializeField]
		private GameObject CrosshairPrefab;

		private CrosshairHandle _crosshair;

		public Vector2 Position
		{
			get
			{
				if (InputManager.Instance != null)
					return InputManager.Instance.CursorWorldPos;

				if (Camera.main == null)
					return Vector2.zero;
				Vector2 screenPos = Vector2.zero;
        
				if (Mouse.current != null)
				{
					screenPos = Mouse.current.position.ReadValue();
				}
				else if (Touchscreen.current != null)
				{
					screenPos = Touchscreen.current.touches[0].position.ReadValue();
				}

				return Camera.main.ScreenToWorldPoint(screenPos);
			}
		}

		private RectTransform _frameTransform, _chargeTransform;
		private float _initFrameSize;

		private float _frameSize;
		private float _chargeScale;

		private FMOD.Studio.PARAMETER_ID _staffChargeParamId;
		private float _lastChargePercent = -1f;
		private bool _systemCursorVisible;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;

			_crosshair = CrosshairPrefab.Clone(transform).GetComponent<CrosshairHandle>();
			if (!PlatformInfo.IsTouch)
				InputManager.Instance?.HideCursor();

			_frameTransform = _crosshair.Frame.GetComponent<RectTransform>();
			_chargeTransform = _crosshair.Charge.GetComponent<RectTransform>();

			_initFrameSize = _frameTransform.sizeDelta.y;

			_frameSize = _initFrameSize;
			_chargeScale = 1f;

			RuntimeManager.StudioSystem.getParameterDescriptionByName("staff-charge", out var staffChargeDesc);
			_staffChargeParamId = staffChargeDesc.id;
		}

		public void Start()
		{
			PlayerManager.Instance.OnPlayerDeath += () =>
			{
				SetCharge(0f);
			};
		}

		public void LateUpdate()
		{
			_crosshair.transform.position = Position;

			if (!PlatformInfo.IsTouch)
				UpdateSystemCursor();
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;

			if (!PlatformInfo.IsTouch)
				InputManager.Instance?.ShowCursor();
		}

		public Vector2 DirectionToCrosshair(Vector2 from)
		{
			return from.DirectionTo(Position);
		}

		public void SetCharge(float percent)
		{
			_chargeScale = percent;
			_chargeTransform.sizeDelta = _chargeScale * _frameSize * Vector2.one;

			float clamped = Mathf.Clamp01(percent);
			if (!Mathf.Approximately(clamped, _lastChargePercent))
			{
				RuntimeManager.StudioSystem.setParameterByID(_staffChargeParamId, clamped);
				_lastChargePercent = clamped;
			}
		}

		private void UpdateSystemCursor()
		{
			bool menuOpen = PauseMenuUI.Instance.IsPaused || PlayerManager.Instance.IsDead;

			if (menuOpen == _systemCursorVisible)
				return;

			_systemCursorVisible = menuOpen;

			if (menuOpen)
				InputManager.Instance.ShowCursor();
			else
				InputManager.Instance.HideCursor();
		}

		public void Show()
		{
			_crosshair.gameObject.SetActive(true);
		}

		public void Hide()
		{
			_crosshair.gameObject.SetActive(false);
		}
	}
}
