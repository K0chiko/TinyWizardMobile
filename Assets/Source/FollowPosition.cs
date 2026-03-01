using UnityEngine;

namespace Quinn
{
	public class FollowPosition : MonoBehaviour
	{
		[field: SerializeField]
		public Transform Target { get; set; }
		[SerializeField]
		private bool KeepOffset;

		private Vector3 _localOffset;

		public void Awake()
		{
			if (KeepOffset)
			{
				_localOffset = transform.localPosition;
			}
		}

		public void LateUpdate()
		{
			if (Target != null)
			{
				transform.position = Target.position + _localOffset;
			}
		}
	}
}
