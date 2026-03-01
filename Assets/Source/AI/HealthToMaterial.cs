using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class HealthToMaterial : MonoBehaviour
	{
		[SerializeField, Required]
		private SpriteRenderer Renderer;
		[SerializeField, Required]
		private Health Health;

		[Space, SerializeField]
		private string Key = "ParamterKeyName";
		[SerializeField]
		private float FactorAtMinHP = 1f, FactorAtMaxHP = 0.1f;

		public void Start()
		{
			Debug.Assert(Renderer != null && Health != null);
			Debug.Assert(Renderer.material.HasFloat(Key));
		}

		public void FixedUpdate()
		{
			float value = Mathf.Lerp(FactorAtMinHP, FactorAtMaxHP, Health.Percent);
			Renderer.material.SetFloat(Key, value);
		}
	}
}
