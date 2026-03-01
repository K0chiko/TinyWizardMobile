using FMODUnity;
using UnityEngine;

namespace Quinn
{
	public class PlaySound : MonoBehaviour
	{
		public void Play(string name)
		{
			RuntimeManager.PlayOneShot(name, transform.position);
		}
	}
}
