using Quinn.DungeonGeneration;
using UnityEngine;

namespace Quinn.AI
{
	public abstract class StaticAgent : MonoBehaviour, IAgent
	{
		public abstract Room Room { get; protected set; }

		public abstract void StartRoom(Room room);
		public abstract void CeaseFire();
	}
}
