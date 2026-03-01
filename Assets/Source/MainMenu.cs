using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn
{
	public class MainMenu : MonoBehaviour
	{
		public void Awake()
		{
			Play();
		}

		public async void Play()
		{
			await SceneManager.LoadSceneAsync(1);
			Global.Bootstrap();
		}
	}
}
