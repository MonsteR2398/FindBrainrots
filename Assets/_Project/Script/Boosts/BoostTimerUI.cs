using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Treasures.Boosts
{
    /// <summary>
    /// Drives the boost timer window. The whole window is shown only while at least one boost
    /// is active; each boost has its own row (icon + fill bar + countdown) that appears only
    /// while that boost is running.
    /// </summary>
    public class BoostTimerUI : MonoBehaviour
    {
        [Header("Window root (shown only when a boost is active)")]
        [SerializeField] private GameObject window;

        [Header("Speed row")]
        [SerializeField] private GameObject speedRow;
        [SerializeField] private Image speedFill;
        [SerializeField] private TextMeshProUGUI speedText;

        [Header("Jump row")]
        [SerializeField] private GameObject jumpRow;
        [SerializeField] private Image jumpFill;
        [SerializeField] private TextMeshProUGUI jumpText;

        private void OnEnable()
        {
            // Start hidden until a boost activates.
            if (window != null) window.SetActive(false);
        }

        private void Update()
        {
            var bc = BoostController.Instance;
            if (bc == null)
            {
                if (window != null && window.activeSelf) window.SetActive(false);
                return;
            }

            bool any = bc.AnyActive();
            if (window != null && window.activeSelf != any)
                window.SetActive(any);

            if (!any) return;

            UpdateRow(bc, BoostType.Speed, speedRow, speedFill, speedText);
            UpdateRow(bc, BoostType.Jump, jumpRow, jumpFill, jumpText);
        }

        private void UpdateRow(BoostController bc, BoostType type, GameObject row, Image fill, TextMeshProUGUI text)
        {
            bool active = bc.IsActive(type);
            if (row != null && row.activeSelf != active)
                row.SetActive(active);

            if (!active) return;

            if (fill != null) fill.fillAmount = bc.GetFraction(type);
            if (text != null) text.text = Mathf.CeilToInt(bc.GetRemaining(type)) + "s";
        }
    }
}
