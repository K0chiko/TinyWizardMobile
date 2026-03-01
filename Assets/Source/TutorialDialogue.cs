using Quinn.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Collider2D))]
	public class TutorialDialogue : MonoBehaviour
	{
		[SerializeField]
		private bool SingleTrigger = true;
		[SerializeField]
		private bool HideDialogue;
		[SerializeField, Multiline, HideIf(nameof(HideDialogue))]
		private string Dialogue = "This is sample dialogue.";

		private bool _hasTriggered;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer() && (!_hasTriggered || !SingleTrigger))
			{
				_hasTriggered = true;

				if (HideDialogue)
				{
					HUD.Instance.HideDialogue();
				}
				else
				{
					HUD.Instance.WriteDialogue(Dialogue);
				}
			}
		}
	}
}
