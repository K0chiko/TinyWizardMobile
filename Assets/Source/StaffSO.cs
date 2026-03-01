using Quinn.PlayerSystem.SpellSystem;
using System.Linq;
using UnityEngine;

namespace Quinn
{
	[CreateAssetMenu(menuName = "Staff Collection")]
	public class StaffsSO : ScriptableObject
	{
		public Staff[] Staffs;

		public Staff GetStaff(string guid)
		{
			var staff = Staffs.FirstOrDefault(x => x.GUID == guid);

			if (staff == null)
			{
				throw new System.ArgumentException($"Failed to find staff with GUID '{guid}' in collection '{name}'");
			}

			return staff;
		}
	}
}
