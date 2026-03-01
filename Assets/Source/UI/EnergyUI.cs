using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace Quinn.UI
{
	public class EnergyUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Slider NormalBar, OverMaxBar;
		[SerializeField, Required]
		private TextMeshProUGUI OverMaxText;
		[SerializeField, Required]
		private VisualEffect OverMaxVFX;

		private PlayerCaster _caster;

		public void Start()
		{
			_caster = PlayerManager.Instance.Player.GetComponent<PlayerCaster>();
		}

		public void Update()
		{
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.H))
			{
				_caster.EquippedStaff.SetEnergy(_caster.EquippedStaff.Energy * 1.5f);
			}
#endif

			if (_caster != null)
			{
				if (_caster.UIStaff.Energy > _caster.UIStaff.MaxEnergy)
				{
					OverMaxText.gameObject.SetActive(true);

					float t = (Mathf.Sin(Time.time) + 1f) / 2f;
					OverMaxText.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, t);
				}
				else
				{
					OverMaxText.gameObject.SetActive(false);
				}

				var staff = _caster.UIStaff;

				if (staff != null)
				{
					float value;
					if (staff.Energy > staff.MaxEnergy)
						value = staff.MaxEnergy / staff.Energy;
					else
						value = staff.Energy / staff.MaxEnergy;

					NormalBar.value = value;
					OverMaxBar.gameObject.SetActive(staff.Energy > staff.MaxEnergy);
				}
				else
				{
					NormalBar.value = 0f;
					OverMaxBar.gameObject.SetActive(false);
				}

				bool enableOvermax = _caster.UIStaff.Energy > _caster.UIStaff.MaxEnergy && HUD.Instance.Alpha > 0.8f;
				OverMaxVFX.SetBool("Enabled", enableOvermax);
			}
		}
	}
}
