using Quinn.UI;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

		[FormerlySerializedAs("MoveAction")]
		[Header("Input System (assign InputActionReferences)")]
		[SerializeField] private InputActionReference moveAction;
		[FormerlySerializedAs("AimAction")] [SerializeField] private InputActionReference aimAction; // Vector2 aim stick (mobile)
		[FormerlySerializedAs("PointerPositionAction")] [SerializeField] private InputActionReference pointerPositionAction; // Optional: screen position (touch/mouse)
		[FormerlySerializedAs("BasicAction")] [SerializeField] private InputActionReference basicAction; // Button
		[FormerlySerializedAs("SpecialAction")] [SerializeField] private InputActionReference specialAction; // Button
		[FormerlySerializedAs("DashAction")] [SerializeField] private InputActionReference dashAction; // Button
		[FormerlySerializedAs("InteractAction")] [SerializeField] private InputActionReference interactAction; // Button
		[FormerlySerializedAs("AimRadius")] [SerializeField] private float aimRadius = 2.5f; // used to synthesize crosshair around player on mobile

		public void Awake()
		{
			Debug.Assert(Instance == null, "There are more than one instances of InputManager!");
			Instance = this;

			// Subscribe to button events (performed/started/canceled)
			if (basicAction != null && basicAction.action != null)
			{
				basicAction.action.started += OnBasicStarted;
				basicAction.action.canceled += OnBasicCanceled;
			}
			if (specialAction != null && specialAction.action != null)
			{
				specialAction.action.started += OnSpecialStarted;
				specialAction.action.canceled += OnSpecialCanceled;
			}
			if (dashAction != null && dashAction.action != null)
			{
				dashAction.action.performed += OnDashPerformed;
			}
			if (interactAction != null && interactAction.action != null)
			{
				interactAction.action.performed += OnInteractPerformed;
			}
		}

		public void OnEnable()
		{
			// Enable actions so they can read values
			EnableAction(moveAction);
			EnableAction(aimAction);
			EnableAction(pointerPositionAction);
			EnableAction(basicAction);
			EnableAction(specialAction);
			EnableAction(dashAction);
			EnableAction(interactAction);
		}

		public void OnDisable()
		{
			DisableAction(moveAction);
			DisableAction(aimAction);
			DisableAction(pointerPositionAction);
			DisableAction(basicAction);
			DisableAction(specialAction);
			DisableAction(dashAction);
			DisableAction(interactAction);
		}

		public void Update()
		{
			if (PauseMenuUI.Instance.IsPaused) return;

			// 1. Движение
			MoveDirection = moveAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
			MoveDirection = Vector2.ClampMagnitude(MoveDirection, 1f);

			// 2. Логика прицеливания (Mobile First)
			Vector2 playerPos = PlayerManager.Instance != null 
				? (Vector2)PlayerManager.Instance.Player.transform.position 
				: Vector2.zero;

			Vector2 aimInput = aimAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;

			if (aimInput.sqrMagnitude > 0.01f)
			{
				// Стик отклонен: выносим прицел на радиус вокруг игрока
				CursorWorldPos = playerPos + aimInput.normalized * aimRadius;
			}
			else
			{
				// Стик отпущен: прицел следует за игроком на фиксированном расстоянии 
				// или по направлению последнего движения
				if (MoveDirection.sqrMagnitude > 0.01f)
				{
					CursorWorldPos = playerPos + MoveDirection.normalized * aimRadius;
				}
				else 
				{
					// Если совсем стоим, просто держим прицел чуть впереди/сверху
					// (или можно оставить CursorWorldPos без изменений, чтобы он не прыгал)
					CursorWorldPos = playerPos + Vector2.up * 0.1f; 
				}
			}
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;

			if (basicAction != null && basicAction.action != null)
			{
				basicAction.action.started -= OnBasicStarted;
				basicAction.action.canceled -= OnBasicCanceled;
			}
			if (specialAction != null && specialAction.action != null)
			{
				specialAction.action.started -= OnSpecialStarted;
				specialAction.action.canceled -= OnSpecialCanceled;
			}
			if (dashAction != null && dashAction.action != null)
			{
				dashAction.action.performed -= OnDashPerformed;
			}
			if (interactAction != null && interactAction.action != null)
			{
				interactAction.action.performed -= OnInteractPerformed;
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
