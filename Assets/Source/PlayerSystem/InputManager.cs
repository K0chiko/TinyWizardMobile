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
		[SerializeField] private InputActionReference moveAction;
		[SerializeField] private InputActionReference aimAction;
		[SerializeField] private InputActionReference basicAction;
		[SerializeField] private InputActionReference specialAction;
		[SerializeField] private InputActionReference dashAction;
		[SerializeField] private InputActionReference interactAction;
		[SerializeField] private InputActionReference pauseMenuAction;
		[SerializeField] private float aimRadius = 2.5f;

		public void Awake()
		{
			Debug.Assert(Instance == null, "There are more than one instances of InputManager!");
			Instance = this;

			BindButtons(true);
		}

		public void OnEnable()
		{
			foreach (var actionRef in AllActions())
				EnableAction(actionRef);
		}

		public void OnDisable()
		{
			foreach (var actionRef in AllActions())
				DisableAction(actionRef);
		}

		public void Update()
		{
			if (PauseMenuUI.Instance.IsPaused) return;
			
			MoveDirection = moveAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
			MoveDirection = Vector2.ClampMagnitude(MoveDirection, 1f);

			Aim();
		}

		private void Aim()
		{
			Vector2 playerPos = PlayerManager.Instance != null 
				? (Vector2)PlayerManager.Instance.Player.transform.position 
				: Vector2.zero;

			Vector2 aimInput = aimAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
			float threshold = 0.2f;

			if (aimInput.magnitude > threshold)
			{
				CursorWorldPos = playerPos + aimInput.normalized * aimRadius;
        
				if (!IsCastHeld) 
				{
					IsCastHeld = true;
					OnCastStart?.Invoke(); 
				}
			}
			else
			{
				if (IsCastHeld)
				{
					IsCastHeld = false;
					OnCastStop?.Invoke(); 
				}
				
				if (MoveDirection.sqrMagnitude > 0.01f)
				{
					CursorWorldPos = playerPos + MoveDirection.normalized * aimRadius;
				}
			}
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;

			BindButtons(false);
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
		
		private void OnPauseMenuPerformed(InputAction.CallbackContext _)
		{
			if (PauseMenuUI.Instance != null)
			{
				PauseMenuUI.Instance.TogglePause();
			}
		}

		private InputActionReference[] AllActions()
		{
			return new[] { moveAction, aimAction, basicAction, specialAction, dashAction, interactAction, pauseMenuAction };
		}

		private void BindButtons(bool subscribe)
		{
			Bind(basicAction, subscribe, started: OnBasicStarted, canceled: OnBasicCanceled);
			Bind(specialAction, subscribe, started: OnSpecialStarted, canceled: OnSpecialCanceled);
			Bind(dashAction, subscribe, performed: OnDashPerformed);
			Bind(interactAction, subscribe, performed: OnInteractPerformed);
			Bind(pauseMenuAction, subscribe, performed: OnPauseMenuPerformed);
		}

		private static void Bind(
			InputActionReference actionRef, bool subscribe,
			Action<InputAction.CallbackContext> started = null,
			Action<InputAction.CallbackContext> performed = null,
			Action<InputAction.CallbackContext> canceled = null)
		{
			if (actionRef == null || actionRef.action == null)
				return;

			var action = actionRef.action;

			if (subscribe)
			{
				if (started != null) action.started += started;
				if (performed != null) action.performed += performed;
				if (canceled != null) action.canceled += canceled;
			}
			else
			{
				if (started != null) action.started -= started;
				if (performed != null) action.performed -= performed;
				if (canceled != null) action.canceled -= canceled;
			}
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
