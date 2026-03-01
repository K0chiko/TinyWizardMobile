using Quinn.DungeonGeneration;
using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI.StaticBrains
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class BasicStaticAI : StaticAgent
	{
		enum FaceDir
		{
			Up, Down, Left, Right
		}

		[SerializeField, FoldoutGroup("Sprites"), Required]
		private Sprite FaceUp, FaceDown, FaceLeft, FaceRight;
		[SerializeField]
		private FaceDir Facing = FaceDir.Down;
		[SerializeField, Required]
		private Missile Missile;
		[SerializeField]
		private float FireInterval = 1f;
		[SerializeField]
		private float FireDelay;
		[SerializeField, Required]
		private Transform MissileOrigin;

		[Space, SerializeField]
		private int Count = 1;
		[SerializeField]
		private float Interval = 0f;
		[SerializeField]
		private MissileSpawnBehavior Behaviour;
		[SerializeField]
		private float Spread = 15f;

		private bool _isActive;
		private float _firingStartTime;
		private Vector2 _fireDir;

		public override Room Room { get; protected set; }

		public void Awake()
		{
			var sprite = Facing switch
			{
				FaceDir.Up => FaceUp,
				FaceDir.Down => FaceDown,
				FaceDir.Left => FaceLeft,
				FaceDir.Right => FaceRight,
				_ => throw new System.NotImplementedException()
			};
			GetComponent<SpriteRenderer>().sprite = sprite;

			_fireDir = Facing switch
			{
				FaceDir.Up => Vector2.up,
				FaceDir.Down => Vector2.down,
				FaceDir.Left => Vector2.left,
				FaceDir.Right => Vector2.right,
				_ => throw new System.NotImplementedException()
			};

			_firingStartTime = Time.time + FireDelay;
		}

		public void Update()
		{
			if (Time.time > _firingStartTime && _isActive)
			{
				Cooldown.Call(this, FireInterval, () =>
				{
					MissileManager.Instance.SpawnMissile(gameObject, Missile, MissileOrigin.position, _fireDir,
						Count, Interval, Behaviour, Spread);
				});
			}
		}

		public override void StartRoom(Room room)
		{
			_isActive = true;
			Room = room;
		}

		public override void CeaseFire()
		{
			_isActive = false;
		}
	}
}
