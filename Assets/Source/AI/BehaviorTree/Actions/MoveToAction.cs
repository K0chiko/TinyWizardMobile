using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move To", story: "Move To [Position]", category: "Action", id: "f32a94d3ee9a64315b5d2513c4c95542")]
	public partial class MoveToAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;

		[SerializeReference]
		public BlackboardVariable<float> StoppingDistance = new(0.1f);
		[SerializeReference]
		public BlackboardVariable<bool> FaceTarget = new(true);

		[SerializeReference]
		public BlackboardVariable<bool> LimitMaxDistance = new(false);
		[SerializeReference]
		public BlackboardVariable<bool> FailOnMaxReached = new(false);
		[SerializeReference]
		public BlackboardVariable<float> MaxDistance = new(5f);

		[SerializeReference]
		public BlackboardVariable<float> ContinueFlowAfterPercent = new(1f);

		private BTAgent _agent;
		private Vector2 _origin;

		protected override Status OnStart()
		{
			if (Position.Value == null)
				return Status.Failure;


			_agent = GameObject.GetComponent<BTAgent>();
			_agent.Animator.SetBool("IsMoving", true);

			_origin = GameObject.transform.position;
			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			bool reached = _agent.Movement.MoveTo(Position.Value, StoppingDistance.Value);

			// If the percent progress made passes a threshold, continue flow of the graph.
			if (GameObject.transform.position.DistanceTo(_origin) / _origin.DistanceTo(Position.Value) >= ContinueFlowAfterPercent.Value)
			{
				reached = true;
			}

			if (LimitMaxDistance.Value && GameObject.transform.position.DistanceTo(_origin) > MaxDistance.Value)
			{
				return FailOnMaxReached.Value ? Status.Failure : Status.Success;
			}

			if (FaceTarget.Value)
			{
				GameObject.transform.localScale = new Vector3(Mathf.Sign(GameObject.transform.position.DirectionTo(Position.Value).x), 1f, 1f);
			}

			return reached ? Status.Success : Status.Running;
		}

		protected override void OnEnd()
		{
			_agent.Animator.SetBool("IsMoving", false);
		}
	}
}
