using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quinn.EvolutionLearning
{
	public class EvolutionManager : MonoBehaviour
	{
		[SerializeField]
		private Playground[] Playgrounds;

		private readonly HashSet<Brain> _brains = new();
		private int _finishedCount;

		private Brain _averageBrain = new();
		private float _mutationFactor = 1f;

		private int _generation;

		private void Awake()
		{
			Debug.Log($"Beginning generation {_generation}...");

			if (_brains.Count > 0)
			{
				var filtered = new HashSet<Brain>();
				float sum = filtered.Sum(x => x.Cost);

				foreach (var brain in _brains)
				{
					Debug.Log($"Filtering brain with cost of {brain.Cost:0.00}...");

					if (brain.Cost / sum < 0.7f)
					{
						filtered.Add(brain);
						Debug.Log("Success!");
					}
					else
					{
						Debug.Log("Failure!");
					}
				}

				Debug.Log($"Filtered down to {filtered.Count} brains.");

				_averageBrain = Brain.Average(filtered.ToArray());
				_brains.Clear();
			}

			foreach (var playground in Playgrounds)
			{
				playground.Manager = this;
				playground.BeginEpisode(_averageBrain.Mutated(_mutationFactor));
			}
		}

		public void PlaygroundFinished(Brain result)
		{
			_brains.Add(result);
			_finishedCount++;

			Debug.Log($"{_finishedCount}/{transform.childCount} agents have finished!");

			if (_finishedCount == transform.childCount)
			{
				_finishedCount = 0;

				_generation++;
				_mutationFactor *= 0.97f;

				int successCount = 0;
                foreach (var playground in Playgrounds)
                {
					if (playground.Success)
						successCount++;
                }
                Debug.Log($"Success percent: {(float)successCount / Playgrounds.Length}!");

				Awake();
			}
		}
	}
}
