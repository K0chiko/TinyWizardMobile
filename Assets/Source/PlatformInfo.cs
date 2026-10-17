using UnityEngine;

namespace Quinn
{
	public static class PlatformInfo
	{
		private static bool? _isTouch;

		public static bool IsTouch => _isTouch ??= Detect();

		private static bool Detect()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorPrefs.GetBool(EditorForceTouchKey, false))
				return true;
#endif
			return Application.isMobilePlatform;
		}

#if UNITY_EDITOR
		public const string EditorForceTouchKey = "Quinn.ForceTouchUi";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetCache()
		{
			_isTouch = null;
		}
#endif
	}
}
