using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn
{
	public class Global : MonoBehaviour
	{
		public static Global Instance;

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
			
			Physics2D.callbacksOnDisable = false;
		}
	}
}
