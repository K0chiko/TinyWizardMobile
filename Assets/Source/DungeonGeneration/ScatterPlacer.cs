using UnityEngine;

namespace Quinn.DungeonGeneration
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class ScatterPlacer : MonoBehaviour
	{
		[SerializeField]
		private bool RandomFacingDir = true;

		public void Start()
		{
			var bounds = GetComponent<BoxCollider2D>().bounds;

			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);

				child.position = new Vector2()
				{
					x = Random.Range(-bounds.extents.x, bounds.extents.y),
					y = Random.Range(-bounds.extents.y, bounds.extents.y)
				};

				child.position += bounds.center;

				if (RandomFacingDir)
				{
					child.localScale = new Vector3(Random.value < 0.5f ? -1f : 1f, 1f, 1f);
				}
			}
		}
	}
}
