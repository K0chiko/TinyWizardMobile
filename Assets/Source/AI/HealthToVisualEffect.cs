using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.AI
{
	public class HealthToVisualEffect : MonoBehaviour
	{
		[SerializeField, Required]
		private Health Health;
		[SerializeField, Required]
		private VisualEffect VFX;

		[Space, SerializeField]
		private string Key = "VisualEffectKeyName";
		[SerializeField]
		private float FactorAtMinHP = 1f, FactorAtMaxHP = 0.1f;

		private float _initValue;

		public void Start()
		{
			Debug.Assert(VFX != null);
			Debug.Assert(VFX.HasFloat(Key));

			_initValue = VFX.GetFloat(Key);
		}

		public void FixedUpdate()
		{
			float factor = Mathf.Lerp(FactorAtMinHP, FactorAtMaxHP, Health.Percent);
			VFX.SetFloat(Key, _initValue * factor);
		}
	}
}
