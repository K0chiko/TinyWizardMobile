using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class RandomSprite : MonoBehaviour
	{
		[SerializeField]
		private Sprite[] Sprites;

		public void Awake()
		{
			if (Sprites.Length > 0)
			{
				GetComponent<SpriteRenderer>().sprite = Sprites.GetRandom();
			}
		}
	}
}
