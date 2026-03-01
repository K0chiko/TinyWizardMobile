using FMODUnity;
using UnityEngine;

namespace Quinn
{
	public static class Audio
	{
		public static void Play(EventReference sound, Vector2 position = default)
		{
			if (!sound.IsNull)
				RuntimeManager.PlayOneShot(sound, position);
		}
		public static void Play(EventReference sound, Transform parent)
		{
			if (!sound.IsNull)
				RuntimeManager.PlayOneShotAttached(sound, parent.gameObject);
		}
	}
}
