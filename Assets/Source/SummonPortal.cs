using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	public class SummonPortal : MonoBehaviour
	{
		[SerializeField, Required]
		private Animator Animator;
		[SerializeField]
		private string DeathTrigger = "Death";

		[Space, SerializeField]
		private float Lifespan = 2f;
		[SerializeField]
		private float DestroyDelay = 3f;

		private float _lifespanEnd;
		private bool _isDestroying;

		public void Awake()
		{
			_lifespanEnd = Time.time + Lifespan;
		}

		public void FixedUpdate()
		{
			if (Time.time > _lifespanEnd && !_isDestroying)
			{
				_isDestroying = true;
				Destroy(gameObject, DestroyDelay);

				Animator.SetTrigger(DeathTrigger);
			}
		}
	}
}
