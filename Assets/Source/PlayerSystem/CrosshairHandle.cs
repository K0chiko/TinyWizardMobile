using UnityEngine;
using UnityEngine.UI;

namespace Quinn.PlayerSystem
{
	public class CrosshairHandle : MonoBehaviour
	{
		[field: SerializeField]
		public Image Frame { get; private set; }
		[field: SerializeField]
		public Image Charge { get; private set; }
	}
}
