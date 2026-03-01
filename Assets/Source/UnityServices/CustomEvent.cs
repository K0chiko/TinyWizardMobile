namespace Quinn.UnityServices
{
	public abstract class CustomEvent : Unity.Services.Analytics.Event
	{
		// "Game Off 2024"
		public const string Prefix = "go24_";

		protected CustomEvent(string name)
			: base(name) { }
	}
}
