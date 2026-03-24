using System.Collections;
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
			StartCoroutine(LoadLevelRoutine());
		}
		
		
		private IEnumerator LoadLevelRoutine()
		{
			yield return SceneManager.LoadSceneAsync(1);
			Debug.Log("Готово!");
		}
	}
	
	


}
