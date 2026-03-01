namespace Quinn.UnityServices.Events
{
	public class DummyEvent : CustomEvent
	{
		public int MyInt { set => SetParameter(Prefix + "my_int", value); }

		protected DummyEvent() :
			base(Prefix + "dummy_event") { }
	}
}
