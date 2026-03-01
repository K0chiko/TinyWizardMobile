using DG.Tweening;
using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class RandomStaff : MonoBehaviour
	{
		[SerializeField]
		private bool AvoidRepeats = true;
		[SerializeField, Required]
		private StaffsSO Staffs;

		public void Start()
		{
			Staff[] filtered;
			Staff activeStaff = PlayerManager.Instance.Player.GetComponent<PlayerCaster>().EquippedStaff;

			if (activeStaff != null && AvoidRepeats)
			{
				filtered = Staffs.Staffs.Where(x => x.GUID != activeStaff.GUID).ToArray();
				filtered = filtered.Where(x => !PlayerManager.Instance.RecentlyLooted.Contains(x.GUID)).ToArray();
			}
			else
			{
				filtered = Staffs.Staffs;
			}

			var selected = filtered.GetRandom();
			var instance = selected.gameObject.Clone(transform);

			var staff = instance.GetComponent<Staff>();
			staff.OnPickedUp += () =>
			{
				transform.DOKill();
				transform.DetachChildren();
				Destroy(gameObject);
			};

			if (AvoidRepeats)
			{
				RegisterLastLooted(staff);
			}
		}

		private void RegisterLastLooted(Staff staff)
		{
			PlayerManager.Instance.RecentlyLooted.Add(staff.GUID);

			if (PlayerManager.Instance.RecentlyLooted.Count > 3)
			{
				PlayerManager.Instance.RecentlyLooted.RemoveAt(0);
			}
		}
	}
}
