using FMODUnity;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.DungeonGeneration
{
	public abstract class Door : MonoBehaviour
	{
		[SerializeField]
		private VisualEffect OpenVFX, CloseVFX;
		[SerializeField]
		private bool StartsClosed;
		[SerializeField]
		private EventReference OpenSound, CloseSound;

		public bool IsOpened { get; private set; } = true;

		public virtual void Awake()
		{
			IsOpened = !StartsClosed;
		}

		public void Open()
		{
			if (!IsOpened)
			{
				IsOpened = true;
				OnOpen();

				Audio.Play(OpenSound);

				if (OpenVFX != null)
				{
					OpenVFX.Play();
				}
			}
		}

		public void Close()
		{
			if (IsOpened)
			{
				IsOpened = false;
				OnClose();

				Audio.Play(CloseSound);

				if (CloseVFX != null)
				{
					CloseVFX.Play();
				}
			}
		}

		protected virtual void OnOpen() { }
		protected virtual void OnClose() { }
	}
}
