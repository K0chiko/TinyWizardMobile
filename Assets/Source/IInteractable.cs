using Quinn.PlayerSystem;

namespace Quinn
{
	public interface IInteractable
	{
		public int Priority => 0;
		public bool ShouldAutoInteract(Player player) => false;
		public void Interact(Player player);
	}
}
