using Quinn.AI.BehaviorTree;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace Quinn.UI
{
	public class BossUI : MonoBehaviour
	{
		public static BossUI Instance { get; private set; }

		[SerializeField, Required]
		private GameObject ElementsParent;
		[SerializeField, Required]
		private TextMeshProUGUI Title;
		[SerializeField, Required]
		private Slider HPBar;
		[SerializeField, Required]
		private VisualEffect HPVFX;

		private BTAgent _boss;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;

			ElementsParent.gameObject.SetActive(false);
		}

		public void FixedUpdate()
		{
			if (_boss != null)
			{
				HPBar.value = _boss.Health.Percent;
				HPVFX.enabled = _boss.Health.Percent < 0.5f;
			}
		}

		public void OnDestroy()
		{
			Debug.Assert(Instance == this);
			Instance = null;
		}

		/// <summary>
		/// Will automatically hide the boss bar upon this agent's death.
		/// </summary>
		public void SetBoss(BTAgent agent)
		{
			_boss = agent;
			Title.text = agent.BossTitle;
			ElementsParent.SetActive(true);

			agent.Health.OnDeath += OnBossDeath;
		}

		private void OnBossDeath()
		{
			_boss = null;
			ElementsParent.SetActive(false);
		}
	}
}
