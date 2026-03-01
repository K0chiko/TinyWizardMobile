using UnityEngine;

namespace Quinn
{
	public class Sway : MonoBehaviour
	{
		[SerializeField]
		private float Frequency = 1f;
		[SerializeField]
		private float Amplitude = 2f;

		private Vector2 _origin;

		public void Awake()
		{
			_origin = transform.position;
		}

		public void Update()
		{
			transform.position = _origin + new Vector2()
			{
				x = Mathf.Sin(Time.time * Frequency) * Amplitude,
				y = Mathf.Cos(Time.time * Frequency) * Amplitude,
			};
		}
	}
}
