using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class GameStart : MonoBehaviour
	{
		public void Start()
		{
			PlayerManager.Instance.GameStart();
		}
	}
}
