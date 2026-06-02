using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ModularTreasures;

namespace Treasures.CurrencySystem
{
    public class CurrencyCounterUI : MonoBehaviour
    {
        [SerializeField] private CurrencyType type;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image iconImage;
        [SerializeField] private float countSpeed = 500f;
        [SerializeField] private bool useSmoothFilling = true;
        [SerializeField] private float maxFillDuration = 1.5f;
        [SerializeField] private NumberFormatMode formatMode = NumberFormatMode.Separated;

        public CurrencyType CurrencyType => type;

        private long _displayedValue;
        private long _targetVisualValue;

        private void OnBalanceChangedHandler(CurrencyType changedType, long newAmount, bool isSilent)
        {
            if (changedType == type)
            {
                // If isSilent is true, we update instantly (no animation expected).
                // If the balance decreased (spending), we also update instantly.
                // Otherwise (normal addition), we wait for AddVisualAmount from the VFX.
                if (isSilent || newAmount < _targetVisualValue)
                {
                    SetInstant(newAmount);
                }
            }
        }

        private void Start()
        {
            CurrencyService.Instance.OnBalanceChanged += OnBalanceChangedHandler;

            var database = CurrencyService.Instance.Database;
            if (database != null)
            {
                var definition = database.GetDefinition(type);
                if (definition != null)
                {
                    if (iconImage != null) iconImage.sprite = definition.Icon;
                }
            }

            _displayedValue = CurrencyService.Instance.GetBalance(type);
            _targetVisualValue = _displayedValue;
            UpdateText(_displayedValue);
        }

        public void AddVisualAmount(long amount)
        {
            long realBalance = CurrencyService.Instance.GetBalance(type);
            _targetVisualValue = System.Math.Min(_targetVisualValue + amount, realBalance);

            if (!useSmoothFilling)
            {
                SetInstant(_targetVisualValue);
                return;
            }

            StopAllCoroutines();
            StartCoroutine(UpdateCounterRoutine());
        }

        public void SetInstant(long value)
        {
            _targetVisualValue = value;
            _displayedValue = value;
            UpdateText(value);
        }

        private IEnumerator UpdateCounterRoutine()
        {
            while (_displayedValue != _targetVisualValue)
            {
                long diff = System.Math.Abs(_targetVisualValue - _displayedValue);
                
                float requiredSpeed = (float)diff / maxFillDuration;
                float currentSpeed = Mathf.Max(countSpeed, requiredSpeed);

                float step = currentSpeed * Time.deltaTime;

                if (diff < step)
                {
                    _displayedValue = _targetVisualValue;
                }
                else
                {
                    _displayedValue += (long)(Mathf.Sign(_targetVisualValue - _displayedValue) * Mathf.Max(1, step));
                }

                UpdateText(_displayedValue);
                yield return null;
            }
        }

        private void UpdateText(long value)
        {
            countText.text = NumberFormatter.Format(value, formatMode);
        }
    }
}
