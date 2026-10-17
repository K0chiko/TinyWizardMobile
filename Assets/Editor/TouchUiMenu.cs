using UnityEditor;

namespace Quinn.Editor
{
	public static class TouchUiMenu
	{
		private const string MenuPath = "Tools/TinyWizard/Force Touch UI In Play Mode";

		[MenuItem(MenuPath)]
		private static void Toggle()
		{
			EditorPrefs.SetBool(PlatformInfo.EditorForceTouchKey, !EditorPrefs.GetBool(PlatformInfo.EditorForceTouchKey, false));
		}

		[MenuItem(MenuPath, true)]
		private static bool ToggleValidate()
		{
			Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PlatformInfo.EditorForceTouchKey, false));
			return !EditorApplication.isPlaying;
		}
	}
}
