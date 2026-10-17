using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quinn.PlayerSystem.SpellSystem
{
	public partial class PlayerCaster
	{
		[Conditional("UNITY_EDITOR")]
		private void ApplyCheats()
		{
			var kb = Keyboard.current;
			if (kb == null)
				return;

			if (kb.digit0Key.wasPressedThisFrame && EquippedStaff != null && EquippedStaff.Energy > 0f)
			{
				EquippedStaff.ConsumeAllEnergy();
			}

			if (kb.digit9Key.wasPressedThisFrame)
			{
				if (EquippedStaff)
				{
					CastingSpark.transform.SetParent(null);
					Destroy(EquippedStaff.gameObject);
					EquippedStaff = null;
				}

				GameObject staff = AllStaffs.Where(x => EquippedStaff == null || x.GUID != EquippedStaff.GUID).GetRandom().gameObject.Clone(StaffPivot);
				EquipStaff(staff.GetComponent<Staff>(), true);
			}
		}
	}
}
