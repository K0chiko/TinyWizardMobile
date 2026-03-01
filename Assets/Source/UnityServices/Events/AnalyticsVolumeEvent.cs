namespace Quinn.UnityServices.Events
{
	public class AnalyticsVolumeEvent : CustomEvent
	{
		public string Name { set => SetParameter("name", value); }

		public AnalyticsVolumeEvent()
			: base(Prefix + "analytics_volume") { }
	}
}
