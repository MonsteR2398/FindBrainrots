using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Treasures.Boosts
{
    /// <summary>
    /// Hook this on a uGUI Button. On click it asks <see cref="BoostController"/> to activate
    /// the configured boost (crystals first, reward ad as fallback).
    /// Optionally shows the crystal cost on a label.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BoostButton : MonoBehaviour
    {
        [SerializeField] private BoostType boostType = BoostType.Speed;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private Image currencyImage;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            if (costLabel != null && BoostController.Instance != null)
            {
                costLabel.text = BoostController.Instance.GetCost(boostType).ToString();
                
                if (transform.Find("SpeedBoost_Cost") is RectTransform rt) 
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                else if (transform.Find("JumpBoost_Cost") is RectTransform rt2)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt2);
                else if (costLabel.transform.parent is RectTransform parentRt)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
            }

            if(currencyImage != null && BoostController.Instance != null)
            {
                currencyImage.sprite = BoostController.Instance.GetIcon();
            }
        }

        private void OnClick()
        {
            if (BoostController.Instance != null)
                BoostController.Instance.RequestBoost(boostType);
            else
                Debug.LogWarning("[Boost] BoostController not present in scene.");
        }
    }
}
