using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.Volumes
{
	[RequireComponent(typeof(Collider2D))]
	public abstract class TriggerVolume : MonoBehaviour
	{
		[SerializeField]
		private bool TriggerOnce = true;

		[SerializeField, Space]
		private bool FilterTeam;
		[SerializeField, ShowIf(nameof(FilterTeam))]
		private Team Team;

		private bool _hasEntered, _hasExited;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (!_hasEntered)
			{
				if (TriggerOnce) 
					_hasEntered = true;

				if (IsTeam(collision))
				{
					OnEnter(collision);
				}
			}
		}

		public void OnTriggerExit2D(Collider2D collision)
		{
			if (!_hasExited)
			{
				if (TriggerOnce)
					_hasExited = true;

				if (IsTeam(collision))
				{
					OnExit(collision);
				}
			}
		}

		public abstract void OnEnter(Collider2D collider);
		public abstract void OnExit(Collider2D collider);

		private bool IsTeam(Collider2D collider)
		{
			if (!FilterTeam)
				return true;

			if (collider.IsPlayer() && Team == Team.Player)
				return true;

			if (collider.IsAI() && Team is Team.Monster or Team.CultistA or Team.CultistB)
				return true;

			return false;
		}
	}
}
