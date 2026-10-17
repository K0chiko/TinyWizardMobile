using FMODUnity;
using Quinn.DungeonGeneration;
using Quinn.PlayerSystem;
using Quinn.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

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

       public Animator Animator { get; private set; }
       public Health Health { get; private set; }
       public AIMovement Movement { get; private set; }
       public Room Room { get; private set; }

       private readonly HashSet<string> _animTriggers = new();
       private bool _inSecondPhase;

       private FMOD.Studio.PARAMETER_ID _secondPhaseParamId;
       private float _lastSecondPhaseValue = -1f;

       private static readonly Collider2D[] s_OverlapBuffer = new Collider2D[8];
       private static readonly ContactFilter2D s_OverlapFilter = new ContactFilter2D().NoFilter();

       public void Awake()
       {
          Animator = GetComponent<Animator>();
          Health = GetComponent<Health>();
          Movement = GetComponent<AIMovement>();

          Health.OnDamaged += OnDamaged;
          Health.OnDeath += OnDeath;

          if (IsBoss)
          {
             RuntimeManager.StudioSystem.getParameterDescriptionByName("second-phase", out var secondPhaseDesc);
             _secondPhaseParamId = secondPhaseDesc.id;
          }
       }

#if UNITY_EDITOR
       public void Update()
       {
          var keyboard = Keyboard.current;
          if (keyboard != null && IsBoss && keyboard.digit7Key.wasPressedThisFrame && Health.Percent > 0.51f)
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
             if (!Mathf.Approximately(phase, _lastSecondPhaseValue))
             {
                RuntimeManager.StudioSystem.setParameterByID(_secondPhaseParamId, phase);
                _lastSecondPhaseValue = phase;
             }
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
       }

       public bool GetAnimationTrigger(string key)
       {
          if (!_animTriggers.Contains(key))
          {
             return false;
          }

          _animTriggers.Remove(key);

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

          int count = Physics2D.OverlapPoint(transform.position, s_OverlapFilter, s_OverlapBuffer);

          for (int i = 0; i < count; i++)
          {
             if (s_OverlapBuffer[i].TryGetComponent(out SoundMaterial soundMat))
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
          instance.setParameterByNameWithLabel("sound-mat", SoundMaterialTypeNames.ToLabel(mat));

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
    }
}