using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Quinn.UI
{
    public class EnergyUI : MonoBehaviour
    {
       [SerializeField, Required]
       private Slider NormalBar, OverMaxBar;
       [SerializeField, Required]
       private TextMeshProUGUI OverMaxText;

       private PlayerCaster _caster;

       private bool _wasOverMaxTextActive;
       private bool _wasOverMaxBarActive;
       private float _lastNormalBarValue = -1f;

       public void Start()
       {
          _caster = PlayerManager.Instance.Player.GetComponent<PlayerCaster>();

          _wasOverMaxTextActive = OverMaxText.gameObject.activeSelf;
          _wasOverMaxBarActive = OverMaxBar.gameObject.activeSelf;
       }

       public void Update()
       {
#if UNITY_EDITOR
          if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
          {
             _caster.EquippedStaff.SetEnergy(_caster.EquippedStaff.Energy * 1.5f);
          }
#endif

          if (_caster != null)
          {
             bool isOverMax = _caster.UIStaff.Energy > _caster.UIStaff.MaxEnergy;

             if (isOverMax)
             {
                if (!_wasOverMaxTextActive)
                {
                   OverMaxText.gameObject.SetActive(true);
                   _wasOverMaxTextActive = true;
                }

                float t = (Mathf.Sin(Time.time) + 1f) / 2f;
                OverMaxText.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, t);
             }
             else if (_wasOverMaxTextActive)
             {
                OverMaxText.gameObject.SetActive(false);
                _wasOverMaxTextActive = false;
             }

             var staff = _caster.UIStaff;

             if (staff != null)
             {
                float value;
                if (staff.Energy > staff.MaxEnergy)
                   value = staff.MaxEnergy / staff.Energy;
                else
                   value = staff.Energy / staff.MaxEnergy;

                if (!Mathf.Approximately(value, _lastNormalBarValue))
                {
                   NormalBar.value = value;
                   _lastNormalBarValue = value;
                }

                bool overMax = staff.Energy > staff.MaxEnergy;
                if (overMax != _wasOverMaxBarActive)
                {
                   OverMaxBar.gameObject.SetActive(overMax);
                   _wasOverMaxBarActive = overMax;
                }
             }
             else
             {
                if (!Mathf.Approximately(_lastNormalBarValue, 0f))
                {
                   NormalBar.value = 0f;
                   _lastNormalBarValue = 0f;
                }

                if (_wasOverMaxBarActive)
                {
                   OverMaxBar.gameObject.SetActive(false);
                   _wasOverMaxBarActive = false;
                }
             }
          }
       }
    }
}