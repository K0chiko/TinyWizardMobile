namespace Quinn.UnityServices.Events
{
	public class StaffEquipEvent : CustomEvent
	{
		public string Name { set => SetParameter("name", value); }

		public StaffEquipEvent()
			: base(Prefix + "staff_equipped") { }
	}
}
