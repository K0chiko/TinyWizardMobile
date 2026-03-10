using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Quinn.AI
{
	public class AISound : MonoBehaviour
	{
		[SerializeField]
		private EventReference Sound;

		[Space, SerializeField]
		private bool HealthControlsParameter;
		[SerializeField]
		private Health Health;
		[SerializeField]
		private string HealthParameterKey;
		[SerializeField]
		private float HealthParamMin = 0f, HealthParamMax = 1f;

		private EventInstance _sound;

		public void Start()
		{
			_sound = RuntimeManager.CreateInstance(Sound);
			RuntimeManager.AttachInstanceToGameObject(_sound, gameObject);
			_sound.start();
		}

		public void OnDisable()
		{
			_sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_sound.release();
		}

		public void FixedUpdate()
		{
			if (HealthControlsParameter)
			{
				float value = Mathf.Lerp(HealthParamMin, HealthParamMax, Health.Percent);
				_sound.setParameterByName(HealthParameterKey, value);
			}
		}

		public void OnDestroy()
		{
			_sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_sound.release();
		}
	}
}
