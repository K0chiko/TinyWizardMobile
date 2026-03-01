using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Dash", story: "Dash in Direction [Direction] at [Speed] m/s for [Distance] m", category: "Action", id: "894349eb80115bea51b98c45d76b6dc9")]
	public partial class DashAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Direction;
		[SerializeReference]
		public BlackboardVariable<float> Speed = new(12f);
		[SerializeReference]
		public BlackboardVariable<float> Distance = new(2f);

		private BTAgent _agent;
		private Vector2 _origin;

		protected override Status OnStart()
		{
			if (Direction.Value == null)
				return Status.Failure;

			_agent = GameObject.GetComponent<BTAgent>();
			_origin = GameObject.transform.position;

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (_origin.DistanceTo(GameObject.transform.position) > Distance.Value)
			{
				return Status.Success;
			}

			_agent.Movement.MoveInDirection(Direction.Value, Speed.Value);
			return Status.Running;
		}
	}
}
