using System;
using Unity.Behavior;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Do Once", story: "Execute only once", category: "Flow", id: "c918d58add4ac356e296e4d13f8a5f14")]
	public partial class DoOnceModifier : Modifier
	{
		private bool _hasDone;

		protected override Status OnStart()
		{
			if (_hasDone)
			{
				return Status.Failure;
			}

			_hasDone = true;
			return StartNode(Child);
		}

		protected override Status OnUpdate()
		{
			return Child.CurrentStatus;
		}
	}
}
