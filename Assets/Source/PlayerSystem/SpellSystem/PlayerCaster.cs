using DG.Tweening;
using FMODUnity;
using Quinn.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(PlayerMovement))]
	public partial class PlayerCaster : MonoBehaviour
	{
		[SerializeField]
		private Transform StaffPivot;
		[SerializeField]
		private float StaffOffset = 0.5f;
		[SerializeField]
		private float InputBufferTimeout = 0.2f;
		[SerializeField]
		private VisualEffect CastingSpark;
		[SerializeField]
		private float EnergyTransferRate = 0.5f;

		[SerializeField, Space]
		private Transform StoredStaffsParent;
		[SerializeField]
		private float StaffMaxAngle = 30f;

		[Space, SerializeField]
		private Staff FallbackStaff;
		[SerializeField]
		private Staff[] AllStaffs, StartingStaffs;

		[field: Space, SerializeField]
		public float MaxMana { get; private set; } = 100f;
		[SerializeField]
		private float ManaRegenDelay = 1f;
		[SerializeField]
		private float ManaRegenRate = 50f;

		[Space, SerializeField]
		[Tooltip("Spotlight shown during an equipping sequence.")]
		private Light2D EquipLight;
		[SerializeField]
		private VisualEffect EquipVFX, EquipEnergyVFX;
		[SerializeField]
		private EventReference EquipSound;

		public Staff EquippedStaff {  get; private set; }
		public Staff UIStaff { get; private set; }
		public bool CanCast => Time.time >= _nextInputTime;

		public float Mana => _mana.Current;

		public bool IsBasicHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		public float Charge { get; private set; }

		public PlayerMovement Movement { get; private set; }
		public event Action<Staff> OnStaffEquipped;

		public event Action<float> OnManaAdded, OnManaRemoved;

		private float _nextInputTime;
		private readonly BufferManager _inputBuffer = new();
		private readonly List<Staff> _storedStaffs = new();

		private PlayerMana _mana;
		private StaffEquipSequence _equipSequence;

		public void Awake()
		{
			Movement = GetComponent<PlayerMovement>();

			_mana = new PlayerMana(MaxMana, ManaRegenDelay, ManaRegenRate);
			_mana.Added += amount => OnManaAdded?.Invoke(amount);
			_mana.Removed += amount => OnManaRemoved?.Invoke(amount);
			_equipSequence = new StaffEquipSequence(this, EquipLight, EquipVFX, EquipEnergyVFX, EquipSound, EnergyTransferRate);

			var input = InputManager.Instance;

			input.OnCastStart += OnBasicStart;
			input.OnCastStop += OnBasicStop;
			input.OnSpecialStart += OnSpecialStart;
			input.OnSpecialStop += OnSpecialStop;

			FallbackStaff.SetStoredState();
			string equippedStaffGUID = PlayerManager.Instance.EquippedStaffGUID;

			if (!string.IsNullOrEmpty(equippedStaffGUID))
			{
				RecreateTransientStaffData(equippedStaffGUID);
			}
			else
			{
				Debug.Assert(StartingStaffs.Length > 0);
				GameObject staff = StartingStaffs.GetRandom().gameObject.Clone();
				EquipStaff(staff.GetComponent<Staff>(), true, true);
			}

			_mana.SetFull();

			if (EquippedStaff != null)
			{
				var manager = PlayerManager.Instance;
				manager.EquippedStaffGUID = EquippedStaff.GUID;
				manager.EquippedStaffEnergy = EquippedStaff.Energy;
			}
		}

		public void Update()
		{
			ApplyCheats();

			if (PauseMenuUI.Instance.IsPaused)
				return;

			if (EquippedStaff != null && EquippedStaff.Energy <= 0f)
			{
				StoreStaff(EquippedStaff);
				EquipStaff(FallbackStaff, true);
			}

			_mana.Tick(EquippedStaff == null || EquippedStaff.CanRegenMana);
		}

		public void LateUpdate()
		{
			UpdateStaffTransform();
			CrosshairManager.Instance.SetCharge(Charge);

			if (PlayerManager.Instance.IsDead)
				return;

			_inputBuffer.Update();

			Vector2 faceDir = transform.position.DirectionTo(CrosshairManager.Instance.Position);

			if (!PauseMenuUI.Instance.IsPaused)
			{
				if (InputManager.Instance.enabled)
					transform.localScale = new Vector3(Mathf.Sign(faceDir.x), 1f, 1f);
			}
		}

		public void OnDestroy()
		{
			UpdateStaffTransientData();

			var input = InputManager.Instance;

			if (input != null)
			{
				input.OnCastStart -= OnBasicStart;
				input.OnSpecialStart -= OnSpecialStart;
			}
		}

		public async void EquipStaff(Staff staff, bool skipSequence = false, bool supressNonSequenceSound = false)
		{
			if (staff != EquippedStaff)
			{
				staff.OnEnergyDepleted += OnStaffEnergyDepleted;

				if (!skipSequence && (EquippedStaff != FallbackStaff))
				{
					await _equipSequence.Play(EquippedStaff, staff, target => UIStaff = target);
				}
				else if (!supressNonSequenceSound)
				{
					Audio.Play(EquipSound, transform.position);
				}

				DequipActiveStaff();

				_storedStaffs.Remove(staff);

				EquippedStaff = staff;
				UIStaff = staff;

				UpdateStaffTransientData();

				staff.transform.SetParent(transform, false);
				staff.SetCaster(this);

				FullyReplenishMana();
				SetCharge(0f);

				CastingSpark.SetGradient("Color", staff.SparkGradient);
				CastingSpark.transform.SetParent(staff.Head, false);
				CastingSpark.transform.localPosition = Vector3.zero;

				OnStaffEquipped?.Invoke(staff);
			}
		}

		public void StoreStaff(Staff staff)
		{
			if (!_storedStaffs.Contains(staff))
			{
				_storedStaffs.Add(staff);
				staff.OnEnergyDepleted -= OnStaffEnergyDepleted;

				if (EquippedStaff == staff)
				{
					DequipActiveStaff();
				}

				staff.transform.SetParent(StoredStaffsParent, false);
				LayoutStoredStaffs();

				staff.DisableInteraction();
				UpdateStaffTransientData();
			}
		}

		public void LayoutStoredStaffs()
		{
			float delta = StaffMaxAngle / _storedStaffs.Count;

			var validStaffs = _storedStaffs.Where(x => x != null).ToArray();

			for (int i = 0; i < validStaffs.Length; i++)
			{
				float angle = delta * i;
				angle -= StaffMaxAngle / 2f;

				var storedStaff = validStaffs[i];
				storedStaff.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.forward));
				storedStaff.transform.localScale = Vector3.one;
			}
		}

		public void DequipActiveStaff()
		{
			if (EquippedStaff != null)
			{
				var previousStaff = EquippedStaff;

				previousStaff.ClearCaster();
				previousStaff.transform.SetParent(null);

				EquippedStaff = null;

				IsBasicHeld = false;
				IsSpecialHeld = false;

				StoreStaff(previousStaff);
				_inputBuffer.Clear();
			}
		}

		public void ConsumeMana(float amount)
		{
			_mana.Consume(amount);
		}

		public void ReplenishMana(float amount)
		{
			_mana.Replenish(amount);
		}

		public void FullyReplenishMana()
		{
			_mana.ReplenishFully();
		}

		public void SetCharge(float percent)
		{
			Charge = percent;
		}

		public void SetCooldown(float duration)
		{
			_nextInputTime = Time.time + duration;
		}

		public void Spark()
		{
			Debug.Assert(CastingSpark != null, "Casting spark has been destroyed! Make sure to detach it before destroying the equipped staff.");
			CastingSpark.Play();
		}

		private void RecreateTransientStaffData(string equippedStaffGUID)
		{
			Staff equippedStaff;

			if (equippedStaffGUID == FallbackStaff.GUID)
			{
				equippedStaff = FallbackStaff;
			}
			else
			{
				GameObject equippedStaffInstance = StaffGUIDToPrefab(equippedStaffGUID).gameObject.Clone();
				equippedStaff = equippedStaffInstance.GetComponent<Staff>();

				StoreStaff(FallbackStaff);
			}

			EquipStaff(equippedStaff, true, true);

			equippedStaff.SetCaster(this);
			equippedStaff.SetEnergy(PlayerManager.Instance.EquippedStaffEnergy);

			foreach (string guid in PlayerManager.Instance.StoredStaffGUIDs)
			{
				GameObject storedStaffInstance = StaffGUIDToPrefab(guid).gameObject.Clone();
				var storedStaff = storedStaffInstance.GetComponent<Staff>();

				StoreStaff(storedStaff);
				storedStaff.SetStoredState();
			}
		}

		private void UpdateStaffTransientData()
		{
			var manager = PlayerManager.Instance;
			if (manager == null) return;

			if (EquippedStaff != null)
			{
				manager.EquippedStaffGUID = EquippedStaff.GUID;
				manager.EquippedStaffEnergy = EquippedStaff.Energy;
			}

			foreach (var staff in _storedStaffs)
			{
				if (staff.GUID != FallbackStaff.GUID)
				{
					manager.StoredStaffGUIDs = _storedStaffs
						.Select(x => x.GUID)
						.Where(x => x != FallbackStaff.GUID)
						.Reverse()
						.ToArray();
				}
			}
		}

		private Staff StaffGUIDToPrefab(string guid)
		{
			var staff = AllStaffs.FirstOrDefault(x => x.GUID == guid);
			Debug.Assert(staff != null, "Failed to find staff. Make sure staff is in AllStaffs array in PlayerCaster!");

			return staff;
		}

		private void OnBasicStart()
		{
			if (EquippedStaff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsBasicHeld = true;
				EquippedStaff.OnBasicDown();
			}, () => CanCast && InputManager.Instance.IsCastHeld);
		}

		private void OnBasicStop()
		{
			if (EquippedStaff == null)
				return;

			if (IsBasicHeld)
			{
				IsBasicHeld = false;
				EquippedStaff.OnBasicUp();
			}
		}

		private void OnSpecialStart()
		{
			if (EquippedStaff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsSpecialHeld = true;
				EquippedStaff.OnSpecialDown();
			}, () => CanCast && InputManager.Instance.IsSpecialHeld);
		}

		private void OnSpecialStop()
		{
			if (EquippedStaff == null)
				return;

			if (IsSpecialHeld)
			{
				IsSpecialHeld = false;
				EquippedStaff.OnSpecialUp();
			}
		}

		private void UpdateStaffTransform()
		{
			if (EquippedStaff == null)
				return;

			EquippedStaff.gameObject.SetActive(PlayerManager.Instance.IsAlive);

			Vector2 cursorPos = InputManager.Instance.CursorWorldPos;

			Vector3 dir = StaffPivot.position.DirectionTo(cursorPos);
			EquippedStaff.transform.position = StaffPivot.position + (dir * StaffOffset);

			float angle = Mathf.Atan2(dir.y, dir.x).ToDegrees();
			EquippedStaff.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			if (dir.x < 0f)
				EquippedStaff.transform.localScale = new Vector3(1f, -1f, 1f);
			else
				EquippedStaff.transform.localScale = Vector3.one;
		}

		private void OnStaffEnergyDepleted(Staff staff)
		{
			var p = PlayerManager.Instance;

			if (p.EquippedStaffGUID == staff.GUID)
			{
				p.EquippedStaffGUID = string.Empty;
			}
		}
	}
}
