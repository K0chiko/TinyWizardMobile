using Quinn.AI.BehaviorTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class BossUI : MonoBehaviour
	{
		public static BossUI Instance { get; private set; }

		[SerializeField]
		private GameObject ElementsParent;
		[SerializeField]
		private TextMeshProUGUI Title;
		[SerializeField]
		private Slider HPBar;

		private BTAgent _boss;
		private float _lastHPBarValue = -1f;

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
				float percent = _boss.Health.Percent;
				if (!Mathf.Approximately(percent, _lastHPBarValue))
				{
					HPBar.value = percent;
					_lastHPBarValue = percent;
				}
			}
		}

		public void OnDestroy()
		{
			Debug.Assert(Instance == this);
			Instance = null;
		}

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
			_lastHPBarValue = -1f;
			ElementsParent.SetActive(false);
		}
	}
}
