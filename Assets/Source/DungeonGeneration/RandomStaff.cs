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
		private bool AvoidPlayersActiveStaff = true;
		[SerializeField, Required]
		private StaffsSO Staffs;

		public void Start()
		{
			Staff[] filtered;
			Staff activeStaff = PlayerManager.Instance.Player.GetComponent<PlayerCaster>().EquippedStaff;

			if (activeStaff != null && AvoidPlayersActiveStaff)
			{
				filtered = Staffs.Staffs.Where(x => x.GUID != activeStaff.GUID).ToArray();
			}
			else
			{
				filtered = Staffs.Staffs;
			}

			var instance = filtered.GetRandom().gameObject.Clone(transform);
			instance.GetComponent<Staff>().OnPickedUp += () =>
			{
				transform.DOKill();
				transform.DetachChildren();
				Destroy(gameObject);
			};
		}
	}
}
