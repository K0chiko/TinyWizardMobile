using DG.Tweening;
using FMODUnity;
using Quinn.DungeonGeneration;
using Quinn.UnityServices;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.PlayerSystem
{
	public class PlayerManager : MonoBehaviour
	{
		[SerializeField, Required, AssetsOnly]
		private GameObject PlayerGroupPrefab;
		[SerializeField]
		private Vector2 InitialSpawnOffset = new(-1f, -1f);
		[SerializeField]
		private EventReference DeathMusicCue;

		public static PlayerManager Instance { get; private set; }

		public Player Player { get; private set; }
		public Health Health { get; private set; }
		public PlayerMovement Movement { get; private set; }

		public List<string> RecentlyLooted { get; } = new();

		public string EquippedStaffGUID { get; set; }
		public float EquippedStaffEnergy { get; set; }
		public string[] StoredStaffGUIDs { get; set; } = Array.Empty<string>();
		public int DiscoveredStaffCount { get; set; }
		public int PathsFound { get; set; }

		public bool IsAlive => !IsDead;
		public bool IsDead => Health == null || Health.IsDead;
		public Vector2 Position => Player.transform.position;

		public int CurrentFloorAttempts { get; private set; }
		public int NewRoomsExploredThisFloor { get; set; }
		public float GameplayDuration => _endTime - _startTime;

		public event Action<Player> OnPlayerSet;
		public event Action<float> OnPlayerHealthChange;
		public event Action OnPlayerMaxHealthChange;

		public event Action OnPlayerDeath;
		public event Action OnPlayerDeathPreSceneLoad;

		private bool _isDead;
		private bool _isGameStarted;

		private float _startTime;
		private float _endTime;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		public void Start()
		{
			DungeonGenerator.Instance.OnFloorStart += _ =>
			{
				CurrentFloorAttempts = 0;
				NewRoomsExploredThisFloor = 0;
			};

			SpawnPlayer(InitialSpawnOffset);
		}

		public void Update()
		{
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Alpha1))
				GoToFloor(0);
			else if (Input.GetKeyDown(KeyCode.Alpha2))
				GoToFloor(1);
			else if (Input.GetKeyDown(KeyCode.Alpha3))
				GoToFloor(2);
			else if (Input.GetKeyDown(KeyCode.Alpha4))
				GoToFloor(3);

			if (Input.GetKeyDown(KeyCode.K))
			{
				Health.Kill();
			}

			if (Input.GetKeyDown(KeyCode.L))
			{
				SceneManager.LoadSceneAsync(2);
			}
#endif
		}

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void GameStart()
		{
			if (!_isGameStarted)
			{
				_isGameStarted = true;
				_startTime = Time.time;
			}
		}

		public void SetPlayer(Player player)
		{
			Player = player;

			if (player == null)
			{
				UnsubscribeAll();
			}
			else
			{
				OnPlayerSet?.Invoke(player);
			}
		}

		public void SpawnPlayer(Vector2 position)
		{
			var playerGroup = PlayerGroupPrefab.Clone(position);

			Health = playerGroup.GetComponentInChildren<Health>();
			Movement = playerGroup.GetComponentInChildren<PlayerMovement>();

			var camManager = GetComponent<CameraManager>();
			camManager.Blackout();

			Health.OnHealed += OnHealed;
			Health.OnDamaged += OnDamaged;
			Health.OnDeath += OnDeath;
			Health.OnMaxChange += OnMaxHealthChange;

			InputManager.Instance.EnableInput();
		}

		public async void RespawnSequence()
		{
			await SceneManager.LoadSceneAsync(1);

			SpawnPlayer(InitialSpawnOffset);
			DungeonGenerator.Instance.StartFloorOfCurrentIndex();
		}

		public void MarkVictoryTime()
		{
			_endTime = Time.time;
		}

		private void OnHealed(float amount)
		{
			OnPlayerHealthChange?.Invoke(amount);
		}

		private void OnDamaged(float amount, Vector2 dir, GameObject source)
		{
			OnPlayerHealthChange?.Invoke(-amount);

			if (Health.Current == 0f && !_isDead)
			{
				_isDead = true;

				Analytics.Instance.Push(new UnityServices.Events.PlayerDeathEvent()
				{
					Name = source.name
				});
			}
		}

		private void OnMaxHealthChange()
		{
			OnPlayerMaxHealthChange?.Invoke();
		}

		private async void OnDeath()
		{
			CurrentFloorAttempts++;

			EquippedStaffGUID = null;
			UnsubscribeAll();

			Player = null;
			Health = null;

			OnPlayerDeath?.Invoke();

			InputManager.Instance.DisableInput();
			Audio.Play(DeathMusicCue);

			var vcam = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
			vcam.GetComponent<CinemachineConfiner2D>().enabled = false;
			var t = DOTween.To(() => vcam.Lens.OrthographicSize, x => vcam.Lens.OrthographicSize = x, 4f, 4f).SetEase(Ease.OutCubic);

			var seq = DOTween.Sequence();
			seq.AppendInterval(1f);
			seq.Append(t);

			await CameraManager.Instance.DeathFadeOut();
			OnPlayerDeathPreSceneLoad?.Invoke();

			Camera.main.enabled = false;
			var gameOver = await Resources.Load<GameObject>("GameOver").CloneAsync();
			DontDestroyOnLoad(gameOver);

			await SceneManager.LoadSceneAsync(1);
		}

		private void UnsubscribeAll()
		{
			if (Health != null)
			{
				Health.OnHealed -= OnHealed;
				Health.OnDamaged -= OnDamaged;
				Health.OnDeath -= OnDeath;
				Health.OnMaxChange -= OnMaxHealthChange;
			}
		}

		private void GoToFloor(int index)
		{
			DungeonGenerator.Instance.SetFloorIndex(index);
			RespawnSequence();
		}
	}
}
