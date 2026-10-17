using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public sealed class RoomDoors
	{
		public bool IsLocked { get; private set; }

		private readonly Transform _owner;
		private readonly EventReference _lockSound, _unlockSound;
		private readonly HashSet<Door> _doors;
		private readonly Door[] _onlyUnlock;

		public RoomDoors(Transform owner, EventReference lockSound, EventReference unlockSound, HashSet<Door> doors, Door[] onlyUnlock)
		{
			_owner = owner;
			_lockSound = lockSound;
			_unlockSound = unlockSound;
			_doors = doors;
			_onlyUnlock = onlyUnlock;
		}

		public void Lock()
		{
			if (IsLocked)
				return;

			IsLocked = true;
			Audio.Play(_lockSound, _owner.position);

			foreach (var door in _doors)
			{
				door.Close();
			}
		}

		public void Unlock()
		{
			if (!IsLocked)
				return;

			IsLocked = false;
			Audio.Play(_unlockSound, _owner.position);

			if (_onlyUnlock.Length > 0)
			{
				foreach (var door in _onlyUnlock)
				{
					door.Open();
				}
			}
			else
			{
				foreach (var door in _doors)
				{
					door.Open();
				}
			}
		}
	}
}
