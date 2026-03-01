using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	public class DisableAllChildren : MonoBehaviour
	{
		[SerializeField]
		private List<GameObject> Exceptions;

		public void Awake()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i).gameObject;

				if (!Exceptions.Contains(child))
				{
					child.SetActive(false);
				}
			}
		}
	}
}
