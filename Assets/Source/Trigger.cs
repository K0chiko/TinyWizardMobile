using UnityEngine;
using UnityEngine.Events;

namespace Quinn
{
	[RequireComponent(typeof(Collider2D))]
	public class Trigger : MonoBehaviour
	{
		[SerializeField]
		private bool FilterForPlayer;
		[SerializeField]
		private UnityEvent OnEnter, OnExit;

		public System.Action<Collider2D> OnTriggerEnter, OnTriggerExit;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (FilterForPlayer && !collision.IsPlayer())
				return;

			OnEnter?.Invoke();
			OnTriggerEnter?.Invoke(collision);
		}

		public void OnTriggerExit2D(Collider2D collision)
		{
			if (FilterForPlayer && !collision.IsPlayer())
				return;

			OnExit?.Invoke();
			OnTriggerExit?.Invoke(collision);
		}
	}
}
