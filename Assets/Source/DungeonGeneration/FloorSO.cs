using FMODUnity;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.DungeonGeneration
{
	[CreateAssetMenu(fileName = "Floor", menuName = "Floor")]
	public class FloorSO : ScriptableObject
	{
		public EventReference Ambience;
		public EventReference Music;
		public VisualEffectAsset AmbientVFX;
		[Multiline]
		public string Title = "Floor\nTitle";
		public bool SkipDropSequence;
		public bool SkipEnterCue;

		[Space]
		public Room StartingRoom;
		public RoomEntry[] Generatable;

		[Space, Range(0f, 1f)]
		public float Reverb;

		[Space, Tooltip("For testing purposes. Doesn't work outside of editor mode.")]
		public GameObject OverrideVariant;
		[Tooltip("Whole floor prefabs.")]
		public GameObject[] Variants;

		public void OnValidate()
		{
			if (Generatable != null)
			{
				float sum = Generatable.Sum(x => x.Weight);

				foreach (var entry in Generatable)
				{
					entry.SumWeight = sum;
				}
			}
		}

		public GameObject GetVariant()
		{
#if UNITY_EDITOR
			if (OverrideVariant != null)
			{
				return OverrideVariant;
			}
#endif

			return Variants.GetRandom();
		}
	}
}
