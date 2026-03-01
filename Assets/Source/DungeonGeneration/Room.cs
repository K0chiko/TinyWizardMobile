using FMOD.Studio;
using FMODUnity;
using Quinn.AI;
using Quinn.AI.BehaviorTree;
using Quinn.AI.Pathfinding;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Quinn.DungeonGeneration
{
	public class Room : MonoBehaviour
	{
		[SerializeField]
		private EventReference LockSound, UnlockSound;

		[SerializeField, Required, Space]
		private Trigger RoomTrigger;
		[SerializeField, Required]
		private CinemachineCamera RoomCamera;
		[SerializeField, Required]
		private Tilemap Navmesh;
		[field: SerializeField, Required]
		public BoxCollider2D PathfindBounds { get; private set; }
		[SerializeField, Required]
		private Transform AgentsParent;

		[SerializeField, Space]
		private bool StartConquered;

		[SerializeField, BoxGroup("Doors")]
		private bool NoDoors;
		[SerializeField, BoxGroup("Doors"), ValidateInput("@ NoDoors || HasNorthDoor || HasEastDoor || HasSouthDoor || HasWestDoor")]
		private Door NorthDoor, SouthDoor, EastDoor, WestDoor;
		[SerializeField]
		private Chest Chest;
		[SerializeField]
		private float PostConquerDelay = 1f;

		[Space, SerializeField, HideIf(nameof(HasCustomMusic))]
		private bool DisableMusic;
		[SerializeField]
		private bool HasCustomMusic;
		[SerializeField, ShowIf(nameof(HasCustomMusic))]
		private EventReference CustomMusic;

		[Space, SerializeField]
		private Collider2D MissileBlocker;

		[Space, SerializeField]
		private bool StartsEnabled;
		[SerializeField]
		private Door[] OnlyUnlock;

		[Space, SerializeField, BoxGroup("Analytics")]
		private bool IsLootRoom;
		[SerializeField, BoxGroup("Analytics")]
		public bool IsBossRoom;
		[SerializeField, BoxGroup("Analytics")]
		private bool IsHealingRoom;

		public bool IsLocked { get; private set; }
		public bool IsConquered { get; private set; }
		public bool IsStarted { get; private set; }

		public bool HasNorthDoor => NorthDoor != null || NoDoors;
		public bool HasEastDoor => EastDoor != null || NoDoors;
		public bool HasSouthDoor => SouthDoor != null || NoDoors;
		public bool HasWestDoor => WestDoor != null || NoDoors;

		public event Action OnRoomConquered;

		public Vector2Int RoomGridIndex { get; set; }

		private readonly HashSet<Door> _doors = new();
		private readonly List<IAgent> _liveAgents = new();
		private readonly HashSet<StaticAgent> _staticAgents = new();

		private EventInstance _customMusic;
		private bool _isBossDead;

		private bool _hasLoaded;
		private bool _isExitQueued;

		private bool _explored;

		public void Awake()
		{
			IsConquered = StartConquered;
			IsStarted = StartConquered;

			if (IsStarted && IsConquered)
			{
				RegisterAllAgents();
			}

			if (StartConquered && MissileBlocker != null)
			{
				Destroy(MissileBlocker.gameObject);
			}

			if (HasNorthDoor)
				_doors.Add(NorthDoor);
			if (HasSouthDoor)
				_doors.Add(SouthDoor);
			if (HasWestDoor)
				_doors.Add(WestDoor);
			if (HasEastDoor)
				_doors.Add(EastDoor);

			RoomTrigger.OnTriggerEnter += OnPlayerTriggerEnter;
			RoomTrigger.OnTriggerExit += OnPlayerTriggerExit;

			_hasLoaded = StartsEnabled;

			if (!StartsEnabled)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(false);
				}
			}
		}

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer() && !_hasLoaded)
			{
				_hasLoaded = true;

				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(true);
				}
			}
		}

		public void Unlock()
		{
			if (!IsLocked)
				return;

			IsLocked = false;
			Audio.Play(UnlockSound, transform.position);

			if (OnlyUnlock.Length > 0)
			{
				foreach (var door in OnlyUnlock)
				{
					door.Open();
				}
			}
			else
			{
				foreach (var door in _doors)
				{
					door.Open();
				}
			}
		}

		public void Lock()
		{
			if (IsLocked)
				return;

			IsLocked = true;
			Audio.Play(LockSound, transform.position);

			foreach (var door in _doors)
			{
				door.Close();
			}
		}

		public void RegisterAgent(IAgent agent)
		{
			agent.StartRoom(this);

			if (agent is MonoBehaviour mono && mono.TryGetComponent(out Health health))
			{
				_liveAgents.Add(agent);
				health.OnDeath += () => OnAgentDeath(agent);
			}
			else if (agent is StaticAgent staticAgent)
			{
				_staticAgents.Add(staticAgent);
			}

			if (agent is BTAgent bt && bt.IsBoss)
			{
				bt.Health.OnDeath += () =>
				{
					_customMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					_customMusic.release();
				};
			}
		}

		public void KillAllLiveAgents()
		{
			foreach (var agent in new List<IAgent>(_liveAgents))
			{
				if (agent != null)
				{
					var health = (agent as MonoBehaviour).GetComponent<Health>();

					if (health.IsAlive)
					{
						health.Kill();
					}
				}
			}
		}

		private void OnPlayerTriggerEnter(Collider2D collider)
		{
			if (IsStarted && !IsConquered)
				return;

			if (collider.IsPlayer())
			{
				if (!_explored)
				{
					_explored = true;
					PlayerManager.Instance.NewRoomsExploredThisFloor++;

					UnityServices.Analytics.Instance.Push(new UnityServices.Events.EnterRoomEvent()
					{
						RoomsExploredThisFloor = PlayerManager.Instance.NewRoomsExploredThisFloor,
						IsLootRoom = IsLootRoom,
						IsBossRoom = IsBossRoom,
						IsHealingRoom = IsHealingRoom
					});
				}

				RoomCamera.enabled = true;
				RoomCamera.Target.TrackingTarget = CameraManager.Instance.CameraTarget;

				Pathfinder.Instance.SetNavmesh(Navmesh);
				RoomManager.Instance.SetActiveRoom(this);

				if (!IsConquered)
				{
					StartRoomEncounter();
				}

				if (!_isBossDead)
				{
					if (DisableMusic || HasCustomMusic)
					{
						RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f);
					}
					else if (!DisableMusic && !HasCustomMusic)
					{
						RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);
					}

					if (HasCustomMusic)
					{
						_customMusic = RuntimeManager.CreateInstance(CustomMusic);
						_customMusic.start();
					}
				}
			}
		}

		private /*async*/ void OnPlayerTriggerExit(Collider2D collider)
		{
			if (collider.IsPlayer() && (!_isExitQueued || IsConquered))
			{
				OnPlayerExitRoom();

				//if (!IsConquered && IsStarted)
				//{
				//	_isExitQueued = true;
				//	await Wait.Until(() => IsConquered);
				//}
			}
		}

		private void StartRoomEncounter()
		{
			IsStarted = true;
			Lock();

			if (MissileBlocker != null)
				Destroy(MissileBlocker.gameObject);

			RegisterAllAgents();
			PlayerManager.Instance.OnPlayerDeath += OnPlayerDeath;
		}

		private void OnPlayerDeath()
		{
			_customMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_customMusic.release();
		}

		private async void OnAgentDeath(IAgent agent)
		{
			if (agent is BTAgent bt && bt.IsBoss)
			{
				_isBossDead = true;
			}

			_liveAgents.Remove(agent);

			if (_liveAgents.Count == 0)
			{
				IsConquered = true;

				foreach (var staticAgent in _staticAgents)
				{
					staticAgent.CeaseFire();
				}

				OnRoomConquered?.Invoke();

				await Wait.Seconds(PostConquerDelay);

				if (Chest != null)
				{
					Chest.Unlock();
				}

				Unlock();
			}
		}

		private void RegisterAllAgents()
		{
			for (int i = 0; i < AgentsParent.childCount; i++)
			{
				Register(AgentsParent.GetChild(i));
			}
		}

		private void Register(Transform parent)
		{
			if (parent.TryGetComponent(out IAgent agent))
			{
				RegisterAgent(agent);
			}

			for (int i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);

				if (child.TryGetComponent(out IAgent childAgent))
				{
					RegisterAgent(childAgent);
				}
			}
		}

		private void OnPlayerExitRoom()
		{
			RoomCamera.enabled = false;
			// Always enable music upon leaivng sot that starting room (which don't start with music) will have music in their hallways.
			RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);

			if (HasCustomMusic)
			{
				_customMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				_customMusic.release();
			}
		}
	}
}
