using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn
{
	public class CameraHandle : MonoBehaviour
	{
		[field: SerializeField]
		public Transform CameraTarget { get; private set; }
		[field: SerializeField]
		public Camera View { get; private set; }
		[field: SerializeField]
		public CinemachineCamera VirtualCamera { get; private set; }
		[field: SerializeField]
		public Image Blackout { get; private set; }

		public void Awake()
		{
			CameraManager.Instance.SetCameraHandle(this);
		}
	}
}
