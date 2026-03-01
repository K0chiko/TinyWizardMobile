using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using DG.Tweening;
using TMPro;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "JumpTo", story: "Jump To [Position]", category: "Action", id: "7064ad5fee0843a827110a6160e40e90")]
	public partial class JumpToAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;

		[SerializeReference]
		public BlackboardVariable<float> Height = new(5f);
		[SerializeReference]
		public BlackboardVariable<float> BaseDuration = new(0.5f);
		[SerializeReference]
		public BlackboardVariable<float> DurationRate = new(0.1f);
		[SerializeReference]
		public BlackboardVariable<bool> DisableCollision = new(true);

		[SerializeReference]
		public BlackboardVariable<Transform> Shadow = new();

		[SerializeReference]
		public BlackboardVariable<string> JumpAnimTrigger = new();
		[SerializeReference]
		public BlackboardVariable<string> FallAnimTrigger = new();

		private Tween _jump;
		private Vector2 _origin;

		private bool _hasFallingBegun;

		private Transform _shadowParent;
		private Vector3 _shadowLocalPos;

		protected override Status OnStart()
		{
			_hasFallingBegun = false;

			_origin = GameObject.transform.position;

			float dur = DurationRate.Value * _origin.DistanceTo(Position.Value);
			dur += BaseDuration.Value;

			_jump = GameObject.transform.DOJump(Position.Value, Height.Value, 1, dur)
				.SetEase(Ease.Linear);

			if (DisableCollision)
			{
				GameObject.GetComponent<Collider2D>().enabled = false;
			}

			if (!string.IsNullOrWhiteSpace(JumpAnimTrigger.Value))
			{
				GameObject.GetComponent<Animator>().SetTrigger(JumpAnimTrigger.Value);
			}

			if (Shadow.Value != null)
			{ 
				_shadowParent = Shadow.Value.transform.parent;
				_shadowLocalPos = Shadow.Value.localPosition;

				Shadow.Value.transform.SetParent(null, true);
			}

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			if (!_jump.active || _jump.IsComplete())
			{
				return Status.Success;
			}

			float percent = _jump.ElapsedPercentage();

			if (Shadow.Value != null)
			{
				Shadow.Value.position = Vector2.Lerp(_origin, Position.Value, percent);
			}

			if (!_hasFallingBegun && !string.IsNullOrWhiteSpace(FallAnimTrigger.Value) && percent > 0.5f)
			{
				_hasFallingBegun = true;
				GameObject.GetComponent<Animator>().SetTrigger(FallAnimTrigger.Value);
			}

			return Status.Running;
		}

		protected override void OnEnd()
		{
			if (DisableCollision.Value)
			{
				GameObject.GetComponent<Collider2D>().enabled = true;
			}

			if (Shadow.Value != null)
			{
				Shadow.Value.transform.SetParent(_shadowParent, false);
				Shadow.Value.localPosition = _shadowLocalPos;
			}
		}
	}
}
