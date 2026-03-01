using UnityEngine;

namespace Quinn
{
	public class RandomSpawn : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] Prefabs;

		public void Awake()
		{
			if (transform.childCount > 0)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.Destroy();
				}
			}

			Prefabs.GetRandom().Clone(transform);
		}
	}
}
