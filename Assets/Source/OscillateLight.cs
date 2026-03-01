using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Quinn
{
	[RequireComponent(typeof(Light2D))]
	public class OscillateLight : MonoBehaviour
	{
		[SerializeField]
		private Vector2 Amplitude = new(0.9f, 1.1f);
		[SerializeField]
		private float Frequency = 1f;

		[Space, SerializeField]
		private bool UsePerlin;

		private Light2D _light;
		private float _baseIntensity;

		public void Awake()
		{
			_light = GetComponent<Light2D>();
			_baseIntensity = _light.intensity;
		}

		public void Update()
		{
			float t = Mathf.Sin(Time.time * Frequency);
			t += 1f;
			t /= 2f;

			if (UsePerlin)
			{
				t = Mathf.PerlinNoise1D(Time.time * Frequency);
			}

			_light.intensity = _baseIntensity * Mathf.Lerp(Amplitude.x, Amplitude.y, t);
		}
	}
}