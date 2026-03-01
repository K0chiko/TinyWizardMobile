using Sirenix.OdinInspector;

namespace Quinn.DungeonGeneration
{
	[System.Serializable]
	public record RoomEntry
	{
		[InfoBox("@(Weight / SumWeight * 100f).ToString() + '%'")]
		[HorizontalGroup, HideLabel]
		public Room Prefab;
		[HorizontalGroup(0.1f), HideLabel]
		public float Weight = 100f;

		[UnityEngine.HideInInspector]
		public float SumWeight = 100f;
	}
}
