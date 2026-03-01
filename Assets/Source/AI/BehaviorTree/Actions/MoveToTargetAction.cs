using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move To Target", story: "Move To [Target]", category: "Action", id: "0ece5662d2360cfcff2060c58baac002")]
	public partial class MoveToTargetAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Transform> Target;

		[SerializeReference]
		public BlackboardVariable<float> StoppingDistance = new(0.2f);
		[SerializeReference]
		public BlackboardVariable<bool> FaceTarget = new(true);

		private BTAgent _agent;
		private AIMovement _movement;

		protected override Status OnStart()
		{
			if (Target.Value == null)
			{
				return Status.Failure;
			}

			if (!GameObject.TryGetComponent(out _movement))
			{
				return Status.Failure;
			}

			_agent = GameObject.GetComponent<BTAgent>();
			_agent.Animator.SetBool("IsMoving", true);

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (FaceTarget.Value)
			{
				GameObject.transform.localScale = new Vector3(Mathf.Sign(GameObject.transform.position.DirectionTo(Target.Value.position).x), 1f, 1f);
			}

			bool reached = _movement.MoveTo(Target.Value.position, StoppingDistance.Value);
			return reached ? Status.Success : Status.Running;
		}

		protected override void OnEnd()
		{
			_agent.Animator.SetBool("IsMoving", false);
		}
	}
}
