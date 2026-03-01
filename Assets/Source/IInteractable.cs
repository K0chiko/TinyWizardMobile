using Quinn.PlayerSystem;

namespace Quinn
{
	public interface IInteractable
	{
		public int Priority => 0;
		public void Interact(Player player);
	}
}
