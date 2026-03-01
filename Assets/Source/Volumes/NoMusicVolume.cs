using FMODUnity;
using UnityEngine;

namespace Quinn.Volumes
{
	public class NoMusicVolume : TriggerVolume
	{
		public override void OnEnter(Collider2D collider)
		{
			RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f);
		}

		public override void OnExit(Collider2D collider)
		{
			RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);
		}
	}
}
