using UnityEngine;

namespace Quinn.EvolutionLearning
{
	public record Brain
	{
		public float Weight = 1f;
		public float Bias;
		public float Cost;

		public static Brain Average(params Brain[] brains)
		{
			var avgeragedBrain = new Brain();

			foreach (var brain in brains)
			{
				avgeragedBrain.Weight += brain.Weight;
				avgeragedBrain.Bias += brain.Bias;
			}

			avgeragedBrain.Weight /= brains.Length;
			avgeragedBrain.Bias /= brains.Length;

			return avgeragedBrain;
		}

		public float Evaluate(float input)
		{
			return (input * Weight) + Bias;
		}

		public Brain Mutated(float factor)
		{
			var mutated = this;
			float deviation = 0.3f * factor;

			mutated.Weight += Random.Range(-deviation, deviation);
			mutated.Bias += Random.Range(-deviation, deviation);

			return mutated;
		}
	}
}
