using UnityEngine;

namespace Quinn
{
	public static class MobilePerfBoot
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
		}
	}
}
