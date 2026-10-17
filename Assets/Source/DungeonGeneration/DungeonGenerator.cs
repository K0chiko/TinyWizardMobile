using FMOD.Studio;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quinn.DungeonGeneration
{
	public class DungeonGenerator : MonoBehaviour
	{
		[SerializeField]
		private int MaxRoomSize = 48;

		[SerializeField, RequiredListLength(MinLength = 1)]
		private FloorSO[] Floors;

		public static DungeonGenerator Instance { get; private set; }

		public FloorSO ActiveFloor { get; private set; }

		private readonly Dictionary<Vector2Int, GameObject> _generatedRooms = new();
		private EventInstance _ambience, _music;

		private GameObject _lastGeneratedRoomPrefab;

		private int _floorIndex;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		public async void Start()
		{
			PlayerManager.Instance.OnPlayerDeath += OnPlayerDeath;
			PlayerManager.Instance.OnPlayerDeathPreSceneLoad += OnPlayerDeathPreSceneLoad;

			await StartFloorAsync(Floors[0]);
		}

#if UNITY_EDITOR
		public void Update()
		{
			if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
			{
				var player = PlayerManager.Instance.Player;
				if (player == null) return;

				var roomObject = GameObject.FindGameObjectsWithTag("Room")
					.FirstOrDefault(x => x.GetComponent<Room>()?.IsBossRoom == true);

				if (roomObject != null)
				{
					Vector3 pos = roomObject.transform.position;
					pos += (Vector3)Vector2.down * 4f;
					player.transform.position = pos;
				}
			}
		}
#endif

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public async void StartFloorOfCurrentIndex()
		{
			await StartFloorAsync(Floors[_floorIndex]);
		}

		public void IncrementFloorIndex()
		{
			_floorIndex++;

			if (_floorIndex >= Floors.Length)
			{
				Debug.Log("Game Finished!");
			}
		}

		public void SetFloorIndex(int i)
		{
			_floorIndex = Mathf.Min(i, Floors.Length - 1);
		}

		private async Awaitable StartFloorAsync(FloorSO floor)
		{
			CameraManager.Instance.Blackout();
			RuntimeManager.StudioSystem.setParameterByName("reverb", floor.Reverb);

			DestroyAllRooms();
			ActiveFloor = floor;

			if (_ambience.isValid())
			{
				_ambience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}

			if (!floor.Ambience.IsNull)
			{
				_ambience = RuntimeManager.CreateInstance(floor.Ambience);
				_ambience.start();
			}

			if (_music.isValid())
			{
				_music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}

			if (!floor.Music.IsNull)
			{
				RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f, ignoreseekspeed: true);

				_music = RuntimeManager.CreateInstance(floor.Music);
				_music.start();
			}

			await floor.GetVariant().CloneAsync();

			var fade = CameraManager.Instance.FadeIn();

			if (!floor.SkipDropSequence)
			{
				var player = PlayerManager.Instance.Player;

				if (floor.AmbientVFX != null)
				{
					player.SetAmbientVFX(floor.AmbientVFX);
				}
				else
				{
					player.ClearAmbientVFX();
				}

				await player.EnterFloorAsync();
			}

			await fade;
		}

		private async Awaitable<Room> GenerateRoomAsync(Room prefab, int x, int y)
		{
			Vector2 pos = RoomGridToWorld(x, y) - Vector2.one;
			var instance = await prefab.gameObject.CloneAsync(pos, Quaternion.identity, transform);

			if (instance == null)
			{
				throw new System.NullReferenceException("Failed to generate room!");
			}

			_generatedRooms.Add(new(x, y), instance);

			var room = instance.GetComponent<Room>();
			room.RoomGridIndex = new(x, y);

			return room;
		}

		private Vector2 RoomGridToWorld(int x, int y)
		{
			return new Vector2(x, y) * MaxRoomSize;
		}

		private void DestroyAllRooms()
		{
			foreach (var room in _generatedRooms)
			{
				Destroy(room.Value);
			}

			_generatedRooms.Clear();
		}

		private void OnPlayerDeath()
		{
			_music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_music.release();
		}

		private void OnPlayerDeathPreSceneLoad()
		{
			DestroyAllRooms();
		}
	}
}
