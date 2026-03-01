namespace Quinn.UnityServices.Events
{
	public class DiscoveredFloorEvent : CustomEvent
	{
		public string Name { set => SetParameter("name", value); }

		public DiscoveredFloorEvent()
			: base(Prefix + "discovered_floor") { }
	}
}
