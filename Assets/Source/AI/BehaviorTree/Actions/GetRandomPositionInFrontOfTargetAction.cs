using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Get Random Position In Front of Target", story: "Get Random [Position] in Front of [Target]", category: "Action", id: "9ca2a818cfb47487a9353d709904ee4e")]
	public partial class GetRandomPositionInFrontOfTargetAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;
		[SerializeReference]
		public BlackboardVariable<Health> Target;

		[SerializeReference]
		public BlackboardVariable<float> Angle = new(90f);
		[SerializeReference]
		public BlackboardVariable<float> MaxDistanceFromTarget = new(6f);
		[SerializeReference]
		public BlackboardVariable<float> ObstacleBuffer = new(0.5f);

		protected override Status OnStart()
		{
			if (Target.Value == null)
				return Status.Failure;

			Vector2 self = GameObject.transform.position;
			Vector2 target = Target.Value.transform.position;
			Vector2 targetToSelf = target.DirectionTo(self);

			float half = Angle.Value / 2f;
			float angle = UnityEngine.Random.Range(-half, half);
			Vector2 rotated = Quaternion.AngleAxis(angle, Vector3.forward) * targetToSelf;

			Draw.Line(target, self, Color.white, float.PositiveInfinity);
			Draw.Line(target, target + (rotated * 100f), Color.yellow, float.PositiveInfinity);

			Vector2 maxPoint = target + (rotated * MaxDistanceFromTarget.Value);
			var hit = Physics2D.Raycast(target, rotated, float.PositiveInfinity, LayerMask.GetMask("Obstacle"));

			Draw.Line(target, maxPoint, Color.magenta, float.PositiveInfinity);

			if (hit)
			{
				float dst = target.DistanceTo(hit.point);
				dst -= ObstacleBuffer.Value;

				maxPoint = target + (rotated * dst);
			}

			Draw.Line(self, maxPoint, Color.green, float.PositiveInfinity);
			Draw.Rect(GameObject.transform.position, Vector2.one * 0.5f, Color.white, float.PositiveInfinity);

			Position.Value = maxPoint;
			return Status.Success;
		}
	}
}
