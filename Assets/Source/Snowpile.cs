using Quinn.AI.BehaviorTree;
using Quinn.DungeonGeneration;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Animator))]
	public class Snowpile : MonoBehaviour
	{
		[SerializeField, Required]
		private GameObject SnowmanPrefab;
		[SerializeField, Required]
		private Room Room;

		public void Spawn()
		{
			GetComponent<Animator>().SetTrigger("Spawn");
		}

		public async void Clone_Anim()
		{
			var instance = await SnowmanPrefab.CloneAsync(transform.position);

			var agent = instance.GetComponent<BTAgent>();
			agent.StartRoom(Room);

			Room.RegisterAgent(agent);
		}
	}
}
