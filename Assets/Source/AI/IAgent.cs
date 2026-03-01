using Quinn.DungeonGeneration;

namespace Quinn.AI
{
	public interface IAgent
	{
		public Room Room { get; }
		public void StartRoom(Room room);
	}
}
