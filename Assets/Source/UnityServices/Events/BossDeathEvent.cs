namespace Quinn.UnityServices.Events
{
	public class BossDeathEvent : CustomEvent
	{
		public string Name { set => SetParameter("name", value); }
		public int Attempts { set => SetParameter("attemptCount", value); }
		public string Staff { set => SetParameter("staff", value); }

		public BossDeathEvent()
			: base(Prefix + "boss_defeated") { }
	}
}
