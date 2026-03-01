using System;
using Unity.Behavior;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;
using Quinn.AI.BehaviorTree;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Wait Until Room Start", category: "Flow", id: "2600698ef399d89929ae06a18273ad22")]
	public partial class WaitUntilRoomStartModifier : Modifier
	{
		private bool _hasStarted;
		private BTAgent _agent;

		protected override Status OnStart()
		{
			if (Child == null)
				return Status.Failure;

			_agent = GameObject.GetComponent<BTAgent>();
			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (_agent.Room == null)
				return Status.Running;

			if (!_hasStarted && _agent.Room.IsStarted)
			{
				_hasStarted = true;
				var status = StartNode(Child);

				if (status is Status.Success or Status.Failure)
				{
					EndNode(Child);
				}

				return status;
			}

			if (_hasStarted)
			{
				return Child.CurrentStatus;
			}

			return Status.Running;
		}
	}
}
