using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class Singe : MonoBehaviour
	{
		[SerializeField]
		private float Delay = 0f;
		[SerializeField]
		private float Rate = 1f;

		private SpriteRenderer _renderer;
		private float _fadeTime;

		public void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
			_fadeTime = Time.time + Delay;

			transform.localRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
		}

		public void Update()
		{
			if (Time.time < _fadeTime)
				return;

			var color = _renderer.color;
			color.a -= Time.deltaTime * Rate;
			color.a = Mathf.Max(color.a, 0f);

			_renderer.color = color;
		}
	}
}
