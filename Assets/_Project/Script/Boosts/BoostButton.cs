using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Treasures.Boosts
{
    /// <summary>
    /// Hook this on a uGUI Button. On click it asks <see cref="BoostController"/> to activate
    /// the configured boost (stock first, reward ad as fallback).
    /// Shows stock count on a label. If stock == 0, hides the count and shows an ad image instead.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BoostButton : MonoBehaviour
    {
        [SerializeField] private BoostType boostType = BoostType.Speed;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private Image currencyImage;
        [SerializeField] private RectTransform boostCost;

        [Header("Stock Display")]
        [SerializeField] private TextMeshProUGUI stockLabel;
        [SerializeField] private Image adImage;

        private Button _button;
        private bool _subscribed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            TryUnsubscribe();
        }

        private void Start()
        {
            TrySubscribe();

            if (costLabel != null && BoostController.Instance != null)
            {
                costLabel.text = BoostController.Instance.GetCost(boostType).ToString();
                if (boostCost != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(boostCost);
            }

            if (currencyImage != null && BoostController.Instance != null)
            {
                currencyImage.sprite = BoostController.Instance.GetIcon();
            }

            // Force initial UI update
            RefreshUI();
        }

        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (BoostController.Instance == null) return;

            BoostController.Instance.OnStockChanged += OnStockChanged;
            _subscribed = true;

            // Refresh immediately after subscribing
            RefreshUI();
        }

        private void TryUnsubscribe()
        {
            if (!_subscribed) return;
            if (BoostController.Instance == null) return;

            BoostController.Instance.OnStockChanged -= OnStockChanged;
            _subscribed = false;
        }

        private void OnStockChanged(BoostStockEventArgs args)
        {
            if (args.Type == boostType)
                UpdateStockUI(args.NewStock);
        }

        private void RefreshUI()
        {
            if (BoostController.Instance != null)
                UpdateStockUI(BoostController.Instance.GetStock(boostType));
        }

        private void UpdateStockUI(int stock)
        {   
            Debug.Log(stock);
            bool hasStock = stock > 0;
            Debug.Log(hasStock);

            if (stockLabel != null)
            {
                stockLabel.gameObject.SetActive(hasStock);
                if (hasStock)
                    stockLabel.text = stock.ToString();
            }

            if (adImage != null)
                adImage.gameObject.SetActive(!hasStock);
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