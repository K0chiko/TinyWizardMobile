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

		public void Play()
		{
			SceneManager.LoadSceneAsync(1);
		}
	}
}
