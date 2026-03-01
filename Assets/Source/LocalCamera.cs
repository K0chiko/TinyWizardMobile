using Unity.Cinemachine;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Collider2D))]
	public class LocalCamera : MonoBehaviour
	{
		[SerializeField]
		private CinemachineCamera VCam;

		public void Awake()
		{
			VCam.enabled = false;
		}

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				VCam.Target.TrackingTarget = collision.transform;
				VCam.enabled = true;
			}
		}

		public void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				VCam.enabled = false;
			}
		}
	}
}
