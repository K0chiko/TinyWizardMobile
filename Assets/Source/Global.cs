using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn
{
	public class Global : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Bootstrap()
		{
			if (SceneManager.GetActiveScene().buildIndex != 0)
			{
				var asset = Resources.Load<GameObject>("Globals");
				var instance = Instantiate(asset);

				DontDestroyOnLoad(instance);
			}
		}

		public void Awake()
		{
			// Required to avoid issues when exiting playmode in the editor.
			Physics2D.callbacksOnDisable = false;
		}
	}
}
