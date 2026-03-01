using Quinn.DungeonGeneration;
using UnityEngine;

namespace Quinn.AI
{
	public class RoomManager : MonoBehaviour
	{
		public static RoomManager Instance { get; private set; }

		public Room ActiveRoom { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		public void SetActiveRoom(Room room)
		{
			ActiveRoom = room;
		}
	}
}
