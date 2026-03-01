using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class EnableChildOnTrigger : MonoBehaviour
	{
		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(true);
				}
			}
		}
	}
}
