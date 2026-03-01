using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.ThreeD
{
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerController3D : MonoBehaviour
	{
		[SerializeField, Required]
		private Camera Camera;

		[Space, SerializeField]
		private float LookFactor = 1f;
		[SerializeField]
		private float MoveSpeed = 3f;

		private Rigidbody _rb;

		public void Awake()
		{
			_rb = GetComponent<Rigidbody>();
		}

		public void Update()
		{
			var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

			var mouseInput = new Vector2(/* TODO */);
			Camera.transform.localRotation = Quaternion.Euler(-mouseInput.y * LookFactor, mouseInput.x * LookFactor, 0f);

			Vector3 rot = Camera.transform.localRotation.eulerAngles;
			rot.y = Mathf.Clamp(rot.y, -180f, 180f);
			Camera.transform.localRotation = Quaternion.Euler(rot);

			_rb.linearVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * MoveSpeed;
		}
	}
}
