using Quinn.UI;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quinn.PlayerSystem
{
	public class InputManager : MonoBehaviour
	{
		public static InputManager Instance { get; private set; }

		public Vector2 MoveDirection { get; private set; }
		public Vector2 CursorWorldPos { get; private set; }

		public bool IsCastHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		public event Action OnInteract;
		public event Action OnDash;
		public event Action OnCastStart, OnCastStop;
		public event Action OnSpecialStart, OnSpecialStop;

		[Header("Input System (assign InputActionReferences)")]
		[SerializeField] private InputActionReference MoveAction;
		[SerializeField] private InputActionReference AimAction; // Vector2 aim stick (mobile)
		[SerializeField] private InputActionReference PointerPositionAction; // Optional: screen position (touch/mouse)
		[SerializeField] private InputActionReference BasicAction; // Button
		[SerializeField] private InputActionReference SpecialAction; // Button
		[SerializeField] private InputActionReference DashAction; // Button
		[SerializeField] private InputActionReference InteractAction; // Button
		[SerializeField] private float AimRadius = 2.5f; // used to synthesize crosshair around player on mobile

		public void Awake()
		{
			Debug.Assert(Instance == null, "There are more than one instances of InputManager!");
			Instance = this;

			// Subscribe to button events (performed/started/canceled)
			if (BasicAction != null && BasicAction.action != null)
			{
				BasicAction.action.started += OnBasicStarted;
				BasicAction.action.canceled += OnBasicCanceled;
			}
			if (SpecialAction != null && SpecialAction.action != null)
			{
				SpecialAction.action.started += OnSpecialStarted;
				SpecialAction.action.canceled += OnSpecialCanceled;
			}
			if (DashAction != null && DashAction.action != null)
			{
				DashAction.action.performed += OnDashPerformed;
			}
			if (InteractAction != null && InteractAction.action != null)
			{
				InteractAction.action.performed += OnInteractPerformed;
			}
		}

		public void OnEnable()
		{
			// Enable actions so they can read values
			EnableAction(MoveAction);
			EnableAction(AimAction);
			EnableAction(PointerPositionAction);
			EnableAction(BasicAction);
			EnableAction(SpecialAction);
			EnableAction(DashAction);
			EnableAction(InteractAction);
		}

		public void OnDisable()
		{
			DisableAction(MoveAction);
			DisableAction(AimAction);
			DisableAction(PointerPositionAction);
			DisableAction(BasicAction);
			DisableAction(SpecialAction);
			DisableAction(DashAction);
			DisableAction(InteractAction);
		}

		public void Update()
		{
			if (PauseMenuUI.Instance.IsPaused)
				return;

			// Move
			if (MoveAction != null && MoveAction.action != null)
				MoveDirection = MoveAction.action.ReadValue<Vector2>();
			else
				MoveDirection = Vector2.zero;
			MoveDirection = Vector2.ClampMagnitude(MoveDirection, 1f);

			// Cursor / Aim world position
			Vector2? screenPos = null;
			if (PointerPositionAction != null && PointerPositionAction.action != null)
			{
				screenPos = PointerPositionAction.action.ReadValue<Vector2>();
			}
			else if (Mouse.current != null)
			{
				screenPos = Mouse.current.position.ReadValue();
			}

			if (screenPos.HasValue)
			{
				CursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.Value.x, screenPos.Value.y, Mathf.Abs(Camera.main.transform.position.z)));
			}
			else if (AimAction != null && AimAction.action != null)
			{
				var aim = AimAction.action.ReadValue<Vector2>();
				if (aim.sqrMagnitude > 0.0001f && PlayerManager.Instance != null)
				{
					var from = (Vector2)PlayerManager.Instance.transform.position;
					CursorWorldPos = from + aim.normalized * AimRadius;
				}
			}
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;

			if (BasicAction != null && BasicAction.action != null)
			{
				BasicAction.action.started -= OnBasicStarted;
				BasicAction.action.canceled -= OnBasicCanceled;
			}
			if (SpecialAction != null && SpecialAction.action != null)
			{
				SpecialAction.action.started -= OnSpecialStarted;
				SpecialAction.action.canceled -= OnSpecialCanceled;
			}
			if (DashAction != null && DashAction.action != null)
			{
				DashAction.action.performed -= OnDashPerformed;
			}
			if (InteractAction != null && InteractAction.action != null)
			{
				InteractAction.action.performed -= OnInteractPerformed;
			}
		}

		public void EnableInput()
		{
			enabled = true;
		}
		public void DisableInput()
		{
			enabled = false;
			MoveDirection = Vector2.zero;
		}

		public void ShowCursor()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		public void HideCursor()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;
		}

		private void OnBasicStarted(InputAction.CallbackContext _)
		{
			IsCastHeld = true;
			OnCastStart?.Invoke();
		}
		private void OnBasicCanceled(InputAction.CallbackContext _)
		{
			IsCastHeld = false;
			OnCastStop?.Invoke();
		}
		private void OnSpecialStarted(InputAction.CallbackContext _)
		{
			IsSpecialHeld = true;
			OnSpecialStart?.Invoke();
		}
		private void OnSpecialCanceled(InputAction.CallbackContext _)
		{
			IsSpecialHeld = false;
			OnSpecialStop?.Invoke();
		}
		private void OnDashPerformed(InputAction.CallbackContext _)
		{
			OnDash?.Invoke();
		}
		private void OnInteractPerformed(InputAction.CallbackContext _)
		{
			OnInteract?.Invoke();
		}

		private static void EnableAction(InputActionReference actionRef)
		{
			if (actionRef != null && actionRef.action != null && !actionRef.action.enabled)
				actionRef.action.Enable();
		}
		private static void DisableAction(InputActionReference actionRef)
		{
			if (actionRef != null && actionRef.action != null && actionRef.action.enabled)
				actionRef.action.Disable();
		}
	}
}
