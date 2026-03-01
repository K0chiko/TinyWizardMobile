using UnityEngine;

namespace Quinn.EvolutionLearning
{
	public class Playground : MonoBehaviour
	{
		[SerializeField]
		private GameObject GoalPrefab, AgentPrefab;
		[SerializeField]
		private float LineLength = 25f;

		public bool Success { get; private set; }

		public EvolutionManager Manager { get; set; }
		private Agent _childAgent;

		public void BeginEpisode(Brain brain)
		{
			float lineHalf = LineLength / 2f;

			float x = Random.Range(-lineHalf, lineHalf);
			var goal = GoalPrefab.Clone(new Vector2(x, transform.position.y));

			x = Random.Range(-lineHalf, lineHalf);
			var agent = AgentPrefab.Clone(new Vector2(x, transform.position.y)).GetComponent<Agent>();
			agent.Brain = brain;
			agent.Playground = this;
			agent.Goal = goal.transform;

			goal.transform.SetParent(transform, true);
			agent.transform.SetParent(transform, true);

			_childAgent = agent;
		}

		public void EndEpisode(bool success)
		{
			Manager.PlaygroundFinished(_childAgent.Brain);
			_childAgent = null;

			Success = success;

			for (int i = 1; i < transform.childCount; i++)
			{
				Destroy(transform.GetChild(i).gameObject);
			}
		}
	}
}
