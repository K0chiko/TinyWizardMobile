using UnityEngine;

namespace Quinn
{
	public class HideTarget : MonoBehaviour
	{
		[SerializeField]
		private GameObject Target;

		public void Hide()
		{
			Target.SetActive(false);
		}
	}
}
