using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Quinn
{
	public static class Footsteps
	{
		private static readonly Collider2D[] s_OverlapBuffer = new Collider2D[8];

		public static SoundMaterialType Sample(Vector2 position, out bool playVFX)
		{
			SoundMaterialType mat = SoundMaterialType.None;
			int highestPriority = -9999;
			playVFX = false;

			int count = Physics2D.OverlapPoint(position, ContactFilter2D.noFilter, s_OverlapBuffer);

			for (int i = 0; i < count; i++)
			{
				if (s_OverlapBuffer[i].TryGetComponent(out SoundMaterial soundMat) && soundMat.Priority > highestPriority)
				{
					mat = soundMat.Material;
					highestPriority = soundMat.Priority;
					playVFX = soundMat.PlayVFX;
				}
			}

			return mat;
		}

		public static void Play(EventReference sound, Vector3 position, SoundMaterialType mat)
		{
			EventInstance instance = RuntimeManager.CreateInstance(sound);
			instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
			instance.setParameterByNameWithLabel("sound-mat", SoundMaterialTypeNames.ToLabel(mat));

			instance.start();
			instance.release();
		}
	}
}
