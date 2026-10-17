using FMOD.Studio;
using FMODUnity;

namespace Quinn.DungeonGeneration
{
	public sealed class RoomMusic
	{
		private readonly bool _disableMusic, _hasCustomMusic;
		private readonly EventReference _customMusic;
		private EventInstance _instance;

		public RoomMusic(bool disableMusic, bool hasCustomMusic, EventReference customMusic)
		{
			_disableMusic = disableMusic;
			_hasCustomMusic = hasCustomMusic;
			_customMusic = customMusic;
		}

		public void OnPlayerEnter()
		{
			if (_disableMusic || _hasCustomMusic)
			{
				RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f);
			}
			else
			{
				RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);
			}

			if (_hasCustomMusic)
			{
				_instance = RuntimeManager.CreateInstance(_customMusic);
				_instance.start();
			}
		}

		public void OnPlayerExit()
		{
			RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);

			if (_hasCustomMusic)
			{
				StopCustom();
			}
		}

		public void StopCustom()
		{
			_instance.stop(STOP_MODE.ALLOWFADEOUT);
			_instance.release();
		}
	}
}
