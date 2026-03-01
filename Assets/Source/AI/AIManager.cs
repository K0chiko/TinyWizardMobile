using System.Collections.Generic;
using UnityEngine;

namespace Quinn.AI
{
	public class AIManager : MonoBehaviour
	{
		public static AIManager Instance { get; private set; }

		public IEnumerable<AIAgent> AllAgents => _allAgents;
		public IEnumerable<AIAgent> EnvironmentAgents => _environmentAgents;
		public IEnumerable<AIAgent> MonsterAgents => _environmentAgents;
		public IEnumerable<AIAgent> CultistAAgents => _environmentAgents;
		public IEnumerable<AIAgent> CultistBAgents => _environmentAgents;

		private readonly HashSet<AIAgent> _allAgents = new();
		private readonly HashSet<AIAgent> _environmentAgents = new();
		private readonly HashSet<AIAgent> _monsterAgents = new();
		private readonly HashSet<AIAgent> _cultistAAgents = new();
		private readonly HashSet<AIAgent> _cultistBAgents = new();

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void AddAgent(AIAgent agent)
		{
			_allAgents.Add(agent);

			switch (agent.Health.Team)
			{
				case Team.Environment:
				{
					_environmentAgents.Add(agent);
					break;
				}
				case Team.Monster:
				{
					_monsterAgents.Add(agent);
					break;
				}
				case Team.CultistA:
				{
					_cultistAAgents.Add(agent);
					break;
				}
				case Team.CultistB:
				{
					_cultistBAgents.Add(agent);
					break;
				}
			}
		}

		public void RemoveAgent(AIAgent agent)
		{
			_allAgents.Remove(agent);

			switch (agent.Health.Team)
			{
				case Team.Environment:
				{
					_environmentAgents.Remove(agent);
					break;
				}
				case Team.Monster:
				{
					_monsterAgents.Remove(agent);
					break;
				}
				case Team.CultistA:
				{
					_cultistAAgents.Remove(agent);
					break;
				}
				case Team.CultistB:
				{
					_cultistBAgents.Remove(agent);
					break;
				}
			}
		}
	}
}
