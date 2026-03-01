namespace Quinn.AI
{
	public class GruntAI : AIAgent
	{
		private bool _isPathing;

		protected override async void OnThink()
		{
			if (!_isPathing)
			{
				_isPathing = true;
				await Movement.PathTo(Target);
				_isPathing = false;
			}
		}
	}
}
