using UnityEngine;

namespace Quinn
{
	public class ChanceDestroy : MonoBehaviour
	{
		[SerializeField, Range(0f, 1f)]
		private float Chance;

		public void Awake()
		{
			if (Random.value < Chance)
			{
				Destroy(gameObject);
			}
		}
	}
}
