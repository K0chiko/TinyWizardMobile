using DG.Tweening;
using FMODUnity;
using Quinn.PlayerSystem;
using UnityEngine;

namespace Quinn
{
	public class HeartPickup : MonoBehaviour, IInteractable
	{
		[SerializeField]
		private float FullHealChance;
		[SerializeField]
		private Sprite FullHealSprite;

		[SerializeField]
		private EventReference PickupSound, AlreadyFullHPSound;

		private bool _isUsed;
		private bool _isFullHeal;

		public void OnEnable()
		{
			if (Random.value < FullHealChance)
			{
				GetComponent<SpriteRenderer>().sprite = FullHealSprite;
				_isFullHeal = true;
			}
		}

		public void OnDestroy()
		{
			transform.DOKill();
		}

		public async void Interact(Player player)
		{
			if (_isUsed)
				return;

			var hp = player.GetComponent<Health>();

			if (hp.Current < hp.Max)
			{
				_isUsed = true;
				Audio.Play(PickupSound);

				await transform.DOPunchScale(Vector3.one * 1.03f, 0.3f)
					.AsyncWaitForCompletion();

				await transform.DOScale(0f, 0.1f)
					.AsyncWaitForCompletion(); ;

				Destroy(gameObject);

				if (_isFullHeal)
				{
					hp.FullHeal();
				}
				else
				{
					hp.Heal(1);
				}
			}
			else
			{
				Audio.Play(AlreadyFullHPSound);

				transform.DOKill(true);
				transform.DOPunchScale(Vector3.one * 1.05f, 0.5f, elasticity: 0.5f);
				transform.DOShakeScale(0.2f);
			}
		}
	}
}
