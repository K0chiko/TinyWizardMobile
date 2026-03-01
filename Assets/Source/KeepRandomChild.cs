using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace Quinn
{
	public class KeepRandomChild : MonoBehaviour
	{
		[SerializeField, ValidateInput("@ChildWeights.Length == transform.childCount", "The number of specified weights must equal the number of children.")]
		private float[] ChildWeights;

		public void Awake()
		{
			int keep = Random.Range(0, transform.childCount);
			float sum = ChildWeights.Sum();

			for (int i = 0; i < ChildWeights.Length; i++)
			{
				float t = ChildWeights[i] / sum;

				if (Random.value < t)
				{
					keep = i;
					break;
				}
			}

			for (int i = 0; i < transform.childCount; i++)
			{
				if (i != keep)
				{
					Destroy(transform.GetChild(i).gameObject);
				}
			}
		}
	}
}
