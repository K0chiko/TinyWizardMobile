using FMODUnity;
using UnityEngine;

namespace Quinn
{
	public class MusicVolume : MonoBehaviour
	{
		enum MusicMode
		{
			Exploration,
			Combat,
			Loot
		}

		[SerializeField]
		private MusicMode Mode;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				RuntimeManager.StudioSystem.setParameterByNameWithLabel("music-mode", Mode.ToString());
			}
		}
	}
}
