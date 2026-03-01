using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Quinn
{
	[InfoBox("@Name", VisibleIf = "@!string.IsNullOrWhiteSpace(Name)")]
	public class IndirectCaller : MonoBehaviour
	{
		[SerializeField, FoldoutGroup("Name")]
		private string Name;

		[SerializeField, FoldoutGroup("Callbacks"), Unit(Units.Second)]
		private float Delay = 0f;
		[SerializeField, FoldoutGroup("Callbacks")]
		private UnityEvent OnCall;

		public async void Call()
		{
			if (Delay > 0f)
			{
				await Wait.Seconds(Delay, destroyCancellationToken);
			}

			OnCall?.Invoke();
		}
	}
}
