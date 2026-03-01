using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.PlayerSystem
{
	public class CrosshairHandle : MonoBehaviour
	{
		[field: SerializeField, Required]
		public Image Frame { get; private set; }
		[field: SerializeField, Required]
		public Image Charge { get; private set; }
	}
}
