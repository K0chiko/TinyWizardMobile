using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.UI
{
	public class VictoryUI : MonoBehaviour
	{
		[SerializeField, Multiline]
		private string Dialogue;

		public void Start()
		{
			HUD.Instance.WriteDialogue(Dialogue);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public async void MainMenu_Button()
		{
			await Wait.Seconds(0.2f);
			await SceneManager.LoadSceneAsync(0);
		}

		public async void Quit_Button()
		{
			await Wait.Seconds(0.2f);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}
