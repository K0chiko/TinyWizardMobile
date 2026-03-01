using UnityEngine;

namespace Quinn
{

	public class SoundMaterial : MonoBehaviour
	{
		[field: SerializeField]
		public SoundMaterialType Material { get; private set; }
		[field: SerializeField]
		public int Priority { get; private set; }
		[field: SerializeField]
		public bool PlayVFX = true;
	}
}
