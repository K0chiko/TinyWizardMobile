using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class CrosshairManager : MonoBehaviour
	{
		public static CrosshairManager Instance { get; private set; }

		[SerializeField, Required]
		private GameObject CrosshairPrefab;

		private CrosshairHandle _crosshair;

		public Vector2 Position
		{
			get
			{
				if (Camera.main == null) 
					return Vector2.zero;

				return Camera.main.ScreenToWorldPoint(Input.mousePosition);
			}
		}

		private RectTransform _frameTransform, _chargeTransform;
		private float _initFrameSize, _initChargeSize;

		private float _frameSize;
		private float _chargeScale;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;

			_crosshair = CrosshairPrefab.Clone(transform).GetComponent<CrosshairHandle>();
			GetComponent<InputManager>().HideCursor();

			_frameTransform = _crosshair.Frame.GetComponent<RectTransform>();
			_chargeTransform = _crosshair.Charge.GetComponent<RectTransform>();

			_initFrameSize = _frameTransform.sizeDelta.y;
			_initChargeSize = _chargeTransform.sizeDelta.y;

			_frameSize = _initFrameSize;
			_chargeScale = 1f;
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
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public Vector2 DirectionToCrosshair(Vector2 from)
		{
			return from.DirectionTo(Position);
		}

		public void SetScale(float scale)
		{
			_frameSize = _initFrameSize * scale;
			_frameTransform.sizeDelta = Vector2.one * _frameSize;

			_chargeTransform.sizeDelta = _chargeScale * _frameSize * Vector2.one;
		}

		public void SetCharge(float percent)
		{
			_chargeScale = percent;
			_chargeTransform.sizeDelta = _chargeScale * _frameSize * Vector2.one;

			RuntimeManager.StudioSystem.setParameterByName("staff-charge", Mathf.Clamp01(percent));
		}

		public void Show()
		{
			if (_crosshair != null)
			{
				_crosshair.gameObject.SetActive(true);
			}
		}

		public void Hide()
		{
			if (_crosshair.gameObject != null)
			{
				_crosshair.gameObject.SetActive(false);
			}
		}
	}
}
