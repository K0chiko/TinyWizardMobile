using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move To with Speed", story: "Move to [Position] at [Speed] m/s", category: "Action", id: "b928caaa82ab4836433d39c5ce54f123")]
	public partial class MoveToWithSpeedAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;
		[SerializeReference]
		public BlackboardVariable<float> Speed = new(4f);
		[SerializeReference]
		public BlackboardVariable<float> StoppingDistance = new(0.1f);
		[SerializeReference]
		public BlackboardVariable<bool> FaceTarget = new(true);

		private BTAgent _agent;

		protected override Status OnStart()
		{
			if (Position.Value == null)
				return Status.Failure;

			_agent = GameObject.GetComponent<BTAgent>();

			_agent.Animator.SetBool("IsMoving", true);

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (FaceTarget.Value)
			{
				GameObject.transform.localScale = new Vector3(Mathf.Sign(GameObject.transform.position.DirectionTo(Position.Value).x), 1f, 1f);
			}

			bool reached = _agent.Movement.MoveTo(Position.Value, Speed.Value, StoppingDistance.Value);
			return reached ? Status.Success : Status.Running;
		}

		protected override void OnEnd()
		{
			_agent.Animator.SetBool("IsMoving", false);
		}
	}
}
