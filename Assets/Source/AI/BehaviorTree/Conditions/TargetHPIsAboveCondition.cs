using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, Unity.Properties.GeneratePropertyBag]
	[Condition(name: "Target HP is Above", story: "[Target] HP is [Comparer] Than [Health] Percent", category: "Conditions", id: "b27b489c112db159a165818ee46131b7")]
	public partial class TargetHPIsAboveCondition : Condition
	{
		public enum Comparison
		{
			Greater,
			Lesser
		}

		[SerializeReference]
		public BlackboardVariable<Health> Target;
		[SerializeReference]
		public BlackboardVariable<Comparison> Comparer = new(Comparison.Lesser);
		[SerializeReference]
		public BlackboardVariable<float> Health = new(0.5f);

		public override bool IsTrue()
		{
			if (Target.Value == null)
			{
				return false;
			}

			if (Comparer.Value is Comparison.Greater)
			{
				return Target.Value.Percent > Health.Value;
			}
			else
			{
				return Target.Value.Percent <=	Health.Value;
			}
		}
	}
}
