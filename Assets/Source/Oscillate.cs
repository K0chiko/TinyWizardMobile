using UnityEngine;

namespace Quinn
{
	public class Oscillate : MonoBehaviour
	{
		[SerializeField]
		private float MinScale = 1f;
		[SerializeField]
		private float MaxScale = 1.1f;
		[SerializeField]
		private float Frequency = 2f;

		[Space, SerializeField]
		private bool RandomSeed = true;

		private float _seedOffset;

		private void Awake()
		{
			if (RandomSeed)
				_seedOffset = Random.value;
		}

		private void Update()
		{
			float t = (Mathf.Sin((Time.time + _seedOffset) * Frequency) + 1f) / 2f;
			float scale = Mathf.Lerp(MinScale, MaxScale, t);

			transform.localScale = Vector3.one * scale;
		}
	}
}
