using Unity.Services.Analytics;
using UnityEngine;

namespace Quinn.UnityServices
{
	public class Analytics : MonoBehaviour
	{
		public static Analytics Instance { get; private set; }

		public void Awake()
		{
			Debug.Assert(Instance == null, "There should only ever be 1 instance of the Analytics class!");
			Instance = this;

			// Data collection is started inside the 'Services' class.
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		public void Push(string name)
		{
			AnalyticsService.Instance.RecordEvent(name);
		}
		public void Push<T>(T evt) where T : CustomEvent
		{
			AnalyticsService.Instance.RecordEvent(evt);
		}

		//private void Log(string name)
		//{
		//	Debug.Log($"Analytics Pushed: '{name}'!");
		//}
	}
}
