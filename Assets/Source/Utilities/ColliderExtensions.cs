using Quinn.AI;
using Quinn.AI.BehaviorTree;
using UnityEngine;

namespace Quinn
{
	public static class ColliderExtensions
	{
		public static bool IsPlayer(this Collider2D collider)
		{
			return collider.CompareTag("Player");
		}

		public static bool IsAI(this Collider2D collider)
		{
			return collider.TryGetComponent(out AIAgent _) || collider.TryGetComponent(out BTAgent _);
		}

		public static bool IsCharacter(this Collider2D collider)
		{
			return collider.IsPlayer() || collider.IsAI();
		}
	}
}
