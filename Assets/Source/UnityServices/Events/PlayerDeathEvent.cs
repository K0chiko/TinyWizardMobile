namespace Quinn.UnityServices.Events
{
	public class PlayerDeathEvent : CustomEvent
	{
		public string Name { set => SetParameter("name", value); }

		public PlayerDeathEvent()
			: base(Prefix + "player_died") { }
	}
}
