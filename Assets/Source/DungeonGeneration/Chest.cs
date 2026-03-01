using DG.Tweening;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace Quinn.DungeonGeneration
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Chest : MonoBehaviour, IInteractable, IDamageable
	{
		[SerializeField]
		private Sprite[] RandomOpen, RandomClosed;

		[SerializeField, Space]
		private bool StartOpen;
		[SerializeField]
		private bool IsInteractable = true;
		[SerializeField, Required]
		private Sprite OpenSprite;
		[field: SerializeField]
		public bool IsLocked { get; private set; }

		[SerializeField, Required]
		private GameObject ContentHandle;
		[SerializeField]
		private float AnimationOffset = 1f;
		[SerializeField]
		private float AnimationDuration = 2f;
		[SerializeField]
		private Ease AnimationEasing = Ease.Linear;

		[SerializeField, Space, Required]
		private StudioEventEmitter Ambience;
		[SerializeField]
		private VisualEffect OpenVFX;
		[SerializeField, Required]
		private SpriteRenderer ItemGlow;

		[Space, SerializeField]
		private EventReference OpenSound;
		[SerializeField]
		private EventReference OpenMusicCueSound;
		[SerializeField]
		private EventReference HitSound;
		[SerializeField]
		private bool DisableAmbienceOnOpen = true;

		[Space, SerializeField, Required]
		private ProximityShow ProximityHelpHandle;
		[SerializeField, Required]
		private Canvas ProximityHelpTextCanvas;

		public bool IsOpen { get; private set; }
		public int Priority => -100;

		public Team Team => Team.Environment;

		private bool _isOpening;
		private int _randomIndex;

		public void Awake()
		{
			if (StartOpen)
			{
				Open_Internal();
				AnimateContent(true);
			}

			if (RandomClosed.Length > 0)
			{
				_randomIndex = Random.Range(0, RandomClosed.Length);
				GetComponent<SpriteRenderer>().sprite = RandomClosed[_randomIndex];
			}
		}

		public void Start()
		{
			ContentHandle.transform.GetChild(0).gameObject.SetActive(false);
		}

		public void FixedUpdate()
		{
			if (_isOpening && ItemGlow != null)
			{
				if (IsItemGone())
				{
					ItemGlow.DOFade(0f, 0.2f);
				}
			}

			if (IsItemGone() && (ProximityHelpHandle.enabled || ProximityHelpTextCanvas.enabled))
			{
				ProximityHelpHandle.enabled = false;
				ProximityHelpTextCanvas.enabled = false;
			}
		}

		public void LateUpdate()
		{
            if (ProximityHelpHandle.enabled && !IsItemGone())
            {
				Vector3 pos = ContentHandle.transform.GetChild(0).transform.position;

				ProximityHelpTextCanvas.transform.position = pos;
				ProximityHelpHandle.transform.position = pos;
			}
		}

		public void OnDestroy()
		{
			transform.DOKill();
			ItemGlow.DOKill();
			ContentHandle.transform.DOKill();
		}

		public void Open()
		{
			if (!IsOpen)
			{
				Open_Internal();
				Audio.Play(OpenSound, transform.position);
				Audio.Play(OpenMusicCueSound);
				AnimateContent();

				OpenVFX.Play();
			}
		}

		public void Unlock()
		{
			IsLocked = false;
		}

		public bool TakeDamage(DamageInfo info)
		{
			Audio.Play(HitSound, transform.position);

			if (IsLocked)
				return true;

			if (info.SourceTeam == Team.Player && !IsOpen && IsInteractable)
			{
				Open();
			}

			return true;
		}

		public void Interact(Player player)
		{
			if (IsLocked)
				return;

			if (!IsOpen && IsInteractable)
			{
				Open();
			}
		}

		private void Open_Internal()
		{
			var sprite = OpenSprite;

			if (RandomOpen.Length > 0)
			{
				sprite = RandomOpen[_randomIndex];
			}

			IsOpen = true;
			GetComponent<SpriteRenderer>().sprite = sprite;

			if (DisableAmbienceOnOpen)
			{
				Ambience.Stop();
			}
		}

		private void AnimateContent(bool skip = false)
		{
			Debug.Assert(ContentHandle.transform.childCount > 0, "Chest's content handle is missing a child!");

			_isOpening = true;

			ItemGlow.color = new Color(1f, 1f, 1f, 0f);
			//ItemGlow.enabled = true;
			ItemGlow.DOFade(1f, 0.1f);

			// TODO: Implement fully or remove the item glow idea.

			ItemGlow.GetComponent<FollowPosition>().Target = ContentHandle.transform.GetChild(0);

			Transform child = ContentHandle.transform.GetChild(0);
			child.gameObject.SetActive(true);

			float yPos = child.position.y + AnimationOffset;
			var tween = child.DOMoveY(yPos, AnimationDuration)
				.SetEase(AnimationEasing);

			ProximityHelpHandle.enabled = true;

			tween.onUpdate += () =>
			{
				if (IsItemGone())
				{
					child.DOKill();
				}
			};

			tween.onComplete += () => _isOpening = false;

			if (skip)
			{
				tween.Complete();
			}
		}

		private bool IsItemGone()
		{
			return ContentHandle.transform.childCount == 0;
		}
	}
}
