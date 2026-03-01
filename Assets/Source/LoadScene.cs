using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn
{
	public class LoadScene : MonoBehaviour
	{
		[SerializeField]
		private int BuildIndex;

		public async void Awake()
		{
			await SceneManager.LoadSceneAsync(BuildIndex);
		}
	}
}
