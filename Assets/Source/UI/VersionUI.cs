using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	public class VersionUI : MonoBehaviour
	{
		public void Awake()
		{
			GetComponent<TextMeshProUGUI>().text = Application.version;
		}
	}
}
