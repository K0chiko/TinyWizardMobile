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
		private PARAMETER_ID _healthParamId;
		private float _lastHealthParamValue = -1f;

		public void Start()
		{
			_sound = RuntimeManager.CreateInstance(Sound);
			RuntimeManager.AttachInstanceToGameObject(_sound, gameObject);
			_sound.start();

			if (HealthControlsParameter)
			{
				_sound.getDescription(out var desc);
				desc.getParameterDescriptionByName(HealthParameterKey, out var paramDesc);
				_healthParamId = paramDesc.id;
			}
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
				if (!Mathf.Approximately(value, _lastHealthParamValue))
				{
					_sound.setParameterByID(_healthParamId, value);
					_lastHealthParamValue = value;
				}
			}
		}

		public void OnDestroy()
		{
			_sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_sound.release();
		}
	}
}
