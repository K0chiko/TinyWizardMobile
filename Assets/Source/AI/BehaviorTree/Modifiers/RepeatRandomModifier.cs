using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Repeat Random", story: "Repeat [Min] to [Max] Times", category: "Flow", id: "1b736dc6af415d93048f26be3985e6be")]
	public partial class RepeatRandomModifier : Modifier
	{
		[SerializeReference]
		public BlackboardVariable<int> Min = new(1);
		[SerializeReference]
		public BlackboardVariable<int> Max = new(2);

		private int _x;
		private int _count;

		protected override Status OnStart()
		{
			if (Child == null)
			{
				return Status.Failure;
			}

			_x = UnityEngine.Random.Range(Min.Value, Max.Value + 1);
			_count = 0;

			var status = StartNode(Child);

			if (status is Status.Success)
			{
				_count++;

				if (_count >= _x)
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

				if (_count >= _x)
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
