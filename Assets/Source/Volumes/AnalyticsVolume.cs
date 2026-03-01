using Quinn.UnityServices;
using Quinn.UnityServices.Events;
using UnityEngine;

namespace Quinn.Volumes
{
	public class AnalyticsVolume : TriggerVolume
	{
		[SerializeField]
		private string Name;

		public override void OnEnter(Collider2D collider)
		{
			var evnt = new AnalyticsVolumeEvent()
			{
				Name = Name
			};

			Analytics.Instance.Push(evnt);
		}

		public override void OnExit(Collider2D collider) { }
	}
}
