using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Repeat X", story: "Repeat [X] Times", category: "Flow", id: "f02883fb08ac3d17ad3bca12f1d69a5f")]
	public partial class RepeatXModifier : Modifier
	{
		[SerializeReference]
		public BlackboardVariable<int> X = new(2);

		private int _count;

		protected override Status OnStart()
		{
			if (Child == null)
			{
				return Status.Failure;
			}

			_count = 0;

			var status = StartNode(Child);

			if (status is Status.Success)
			{
				_count++;

				if (_count >= X.Value)
				{
					return Status.Success;
				}
			}
			else if (status is Status.Failure)
			{
				return Status.Failure;
			}

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			var status = Child.CurrentStatus;

			if (status is Status.Success)
			{
				_count++;

				if (_count >= X.Value)
				{
					return Status.Success;
				}
				else
				{
					status = StartNode(Child);
				}
			}
			else if (status is Status.Failure)
			{
				return Status.Failure;
			}

			return Status.Running;
		}
	}
}
