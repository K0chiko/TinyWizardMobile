namespace Quinn.UnityServices.Events
{
	public class StaffBrokeEvent : CustomEvent
	{
		public string Name { set => SetParameter("name", value); }

		public StaffBrokeEvent()
			: base(Prefix + "staff_broke") { }
	}
}
