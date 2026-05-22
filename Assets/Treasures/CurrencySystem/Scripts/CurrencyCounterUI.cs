using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasures.CurrencySystem
{
    public enum CurrencyFormatMode
    {
        Raw,         // 1000000
        Separated,   // 1.000.000
        Abbreviated  // 1K, 1M, 1B
    }

    public class CurrencyCounterUI : MonoBehaviour
    {
        [SerializeField] private CurrencyDefinition definition;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image iconImage;
        [SerializeField] private float countSpeed = 500f;
        [SerializeField] private bool useSmoothFilling = true;
        [SerializeField] private float maxFillDuration = 1.5f;
        [SerializeField] private CurrencyFormatMode formatMode = CurrencyFormatMode.Separated;

        public CurrencyType CurrencyType => definition != null ? definition.Type : CurrencyType.Gold;

        private long _displayedValue;
        private long _targetVisualValue;

        private void Start()
        {
            if (definition != null)
            {
                if (iconImage != null) iconImage.sprite = definition.Icon;
                
                _displayedValue = CurrencyService.Instance.GetBalance(definition.Type);
                _targetVisualValue = _displayedValue;
                UpdateText(_displayedValue);
            }
        }

        public void AddVisualAmount(long amount)
        {
            _targetVisualValue += amount;

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
            countText.text = FormatNumber(value);
        }

        private string FormatNumber(long value)
        {
            switch (formatMode)
            {
                case CurrencyFormatMode.Raw:
                    return value.ToString();
                case CurrencyFormatMode.Separated:
                    var nfi = new System.Globalization.NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 0 };
                    return value.ToString("N", nfi);
                case CurrencyFormatMode.Abbreviated:
                    return Abbreviate(value);
                default:
                    return value.ToString();
            }
        }

        private string Abbreviate(long value)
        {
            if (value < 1000) return value.ToString();
            
            var nfi = new System.Globalization.NumberFormatInfo { NumberDecimalSeparator = "." };
            
            if (value < 1000000) 
                return (value / 1000f).ToString("F1", nfi).TrimEnd('0').TrimEnd('.') + "K";
            
            if (value < 1000000000) 
                return (value / 1000000f).ToString("F1", nfi).TrimEnd('0').TrimEnd('.') + "M";
            
            return (value / 1000000000f).ToString("F1", nfi).TrimEnd('0').TrimEnd('.') + "B";
        }
}
}
