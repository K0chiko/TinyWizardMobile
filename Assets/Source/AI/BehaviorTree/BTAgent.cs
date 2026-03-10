using FMODUnity;
using Quinn.DungeonGeneration;
using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Quinn.UI;
using Quinn.UnityServices;
using Quinn.UnityServices.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace Quinn.AI.BehaviorTree
{
	[RequireComponent(typeof(BehaviorGraphAgent))]
	[RequireComponent(typeof(AIMovement))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(Animator))]
	public class BTAgent : MonoBehaviour, IAgent
	{
		[SerializeField]
		private EventReference FootstepSound;
		[SerializeField]
		private string DeathTrigger = "Die";
		[SerializeField]
		private VisualEffect DeathVFX;
		[SerializeField]
		private EventReference DeathSFX;
		[SerializeField, ShowIf("@!DeathSFX.IsNull")]
		private float StopDeathSFXAfter = -1f;
		[SerializeField]
		private GameObject[] SpawnOnDeath;

		[field: SerializeField, FoldoutGroup("Boss")]
		public bool IsBoss { get; private set; }
		[field: SerializeField, FoldoutGroup("Boss"), ShowIf(nameof(IsBoss))]
		public string BossTitle { get; private set; } = "Boss Title";
		[SerializeField, FoldoutGroup("Boss"), ShowIf(nameof(IsBoss))]
		private GameObject[] ActivateOnSecondPhase;
		[SerializeField, FoldoutGroup("Boss"), ShowIf(nameof(IsBoss))]
		private UnityEvent OnSecondPhaseStartEvent, OnDeathEvent;

		[Space, SerializeField]
		private VisualEffect[] PlayableVFX;

		[FoldoutGroup("Debug"), SerializeField]
		private bool PrintAnimationTriggers;

		public Animator Animator { get; private set; }
		public Health Health { get; private set; }
		public AIMovement Movement { get; private set; }
		public Room Room { get; private set; }

		// A trigger is "set" by virtue of being present in this set.
		private readonly HashSet<string> _animTriggers = new();
		private bool _inSecondPhase;

		public void Awake()
		{
			Animator = GetComponent<Animator>();
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();

			Health.OnDamaged += OnDamaged;
			Health.OnDeath += OnDeath;
		}

#if UNITY_EDITOR
		public void Update()
		{
			if (IsBoss && Input.GetKeyDown(KeyCode.Alpha7) && Health.Percent > 0.51f)
			{
				Health.TakeDamage((Health.Max / 2f) + 1f, Vector2.zero, Team.Player, PlayerManager.Instance.Player.gameObject);
			}
		}
#endif

		public void FixedUpdate()
		{
			if (IsBoss)
			{
				float phase = Health.Percent <= 0.5f ? 1f : 0f;
				RuntimeManager.StudioSystem.setParameterByName("second-phase", phase);
			}
		}

		public void StartRoom(Room room)
		{
			Room = room;

			if (IsBoss)
			{
				BossUI.Instance.SetBoss(this);
			}
		}

		public void SetAnimationTrigger(string key)
		{
			_animTriggers.Add(key);

			if (PrintAnimationTriggers)
			{
				Debug.Log($"<color=#{GetColorOfString(key)}>Trigger set: '{key}'!</color>");
			}
		}

		public bool GetAnimationTrigger(string key)
		{
			if (!_animTriggers.Contains(key))
			{
				return false;
			}

			_animTriggers.Remove(key);

			if (PrintAnimationTriggers)
			{
				Debug.Log($"<color=#{GetColorOfString(key)}>Trigger consumed: '{key}'!</color>");
			}

			return true;
		}

		public void OnFootstep_Anim()
		{
			PlayFootstepSound(GetSoundMaterialType());
		}

		public void DisableCharacterCollision()
		{
			GetComponent<Collider2D>().excludeLayers |= LayerMask.GetMask("Character");
		}

		public void EnableCharacterCollision()
		{
			GetComponent<Collider2D>().excludeLayers &= ~LayerMask.GetMask("Character");
		}

		public void PlayVFX(int index)
		{
			PlayableVFX[index].Play();
		}

		private SoundMaterialType GetSoundMaterialType()
		{
			SoundMaterialType mat = SoundMaterialType.None;
			int highestPriority = -9999;

			var colliders = Physics2D.OverlapPointAll(transform.position);

			foreach (var collider in colliders)
			{
				if (collider.TryGetComponent(out SoundMaterial soundMat))
				{
					if (soundMat.Priority > highestPriority)
					{
						mat = soundMat.Material;
						highestPriority = soundMat.Priority;
					}
				}
			}

			return mat;
		}

		private void PlayFootstepSound(SoundMaterialType mat)
		{
			var instance = RuntimeManager.CreateInstance(FootstepSound);
			instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
			instance.setParameterByNameWithLabel("sound-mat", mat.ToString());

			instance.start();
			instance.release();
		}

		private void OnDamaged(float damage, Vector2 direction, GameObject source)
		{
			if (IsBoss && !_inSecondPhase && Health.Percent <= 0.5f)
			{
				_inSecondPhase = true;
				OnSecondPhaseStartEvent?.Invoke();

				foreach (var obj in ActivateOnSecondPhase)
				{
					obj.SetActive(true);
				}
			}
		}

		private async void OnDeath()
		{
			Animator.SetTrigger(DeathTrigger);

			if (DeathVFX != null)
			{
				DeathVFX.Play();
			}

			if (IsBoss)
			{
				Room.KillAllLiveAgents();

				OnDeathEvent?.Invoke();

				Analytics.Instance.Push(new BossDeathEvent()
				{
					Name = BossTitle,
					Attempts = PlayerManager.Instance.CurrentFloorAttempts,
					Staff = PlayerManager.Instance.Player.GetComponent<PlayerCaster>().EquippedStaff.gameObject.name
				});
			}

			foreach (var prefab in SpawnOnDeath)
			{
				prefab.Clone(transform.position);
			}

			if (!DeathSFX.IsNull)
			{
				var sfx = RuntimeManager.CreateInstance(DeathSFX);
				sfx.start();

				if (StopDeathSFXAfter > 0f)
				{
					await Wait.Seconds(StopDeathSFXAfter, destroyCancellationToken);
					sfx.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					sfx.release();
				}
			}
		}

		private string GetColorOfString(string str)
		{
			var rand = new System.Random(str.GetHashCode());
			float hue = (float)rand.NextDouble();

			var color = Color.HSVToRGB(hue, 1f, 1f);
			return color.ToHexString();
		}
	}
}
