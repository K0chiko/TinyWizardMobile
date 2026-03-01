namespace Quinn.UnityServices.Events
{
	public class EnterRoomEvent : CustomEvent
	{
		public int RoomsExploredThisFloor { set => SetParameter("roomsExplored", value); }
		public bool IsLootRoom { set => SetParameter("isLootRoom", value); }
		public bool IsBossRoom { set => SetParameter("isBossRoom", value); }
		public bool IsHealingRoom { set => SetParameter("isHealingRoom", value); }

		public EnterRoomEvent()
			: base(Prefix + "enter_new_room") { }
	}
}
