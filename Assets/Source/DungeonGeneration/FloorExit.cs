using FMODUnity;
using Quinn.PlayerSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.DungeonGeneration
{
	public class FloorExit : MonoBehaviour
	{
		[SerializeField]
		private SpriteMask Mask;
		[SerializeField]
		private bool IsVictoryExit;

		private bool _isTriggered;

		public void Awake()
		{
			Mask.enabled = false;
		}

		public async void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer() && !_isTriggered)
			{
				if (IsVictoryExit)
				{
					RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f);

					await PlayerManager.Instance.Player.ExitFloorAsync(this);
					await SceneManager.LoadSceneAsync(2);

					return;
				}

				_isTriggered = true;
				DungeonGenerator.Instance.IncrementFloorIndex();

				await PlayerManager.Instance.Player.ExitFloorAsync(this);
				PlayerManager.Instance.RespawnSequence();
			}
		}

		public void EnableMask()
		{
			Mask.enabled = true;
		}
	}
}
