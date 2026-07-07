using System;
using System.Collections.Generic;
using UnityEngine;
using Treasures.CurrencySystem;
using Treasures.Services;

namespace Treasures.Boosts
{
    /// <summary>
    /// Arguments for stock-changed events.
    /// </summary>
    public class BoostStockEventArgs : EventArgs
    {
        public BoostType Type { get; }
        public int NewStock { get; }

        public BoostStockEventArgs(BoostType type, int newStock)
        {
            Type = type;
            NewStock = newStock;
        }
    }

    public enum BoostType
    {
        Speed,
        Jump,
        Vision
    }

    /// <summary>
    /// Central manager for temporary player boosts (speed / jump).
    /// - Boosts can be accumulated (stock). If stock > 0, using a boost consumes 1 stock.
    /// - If stock == 0, a rewarded ad is shown to grant 1 stock.
    /// - Re-activating an already active boost STACKS its remaining duration.
    /// - Multiple boost types can be active simultaneously.
    /// UI binds to <see cref="OnBoostsChanged"/> (start/stop) and polls remaining time per frame.
    /// </summary>
    public class BoostController : MonoBehaviour
    {
        [Serializable]
        public class BoostConfig
        {
            [Tooltip("How many seconds the boost lasts per activation.")]
            public float duration = 15f;

            [Tooltip("Multiplier applied while active (speed or jump height).")]
            public float multiplier = 1.6f;

            [Tooltip("Crystal (Diamond) cost to activate.")]
            public int diamondCost = 25;


            [NonSerialized]public bool isActived = false;
        }

        public static BoostController Instance { get; private set; }

        [Header("Balance (tune freely)")]
        [SerializeField] private BoostConfig speedBoost = new BoostConfig { duration = 15f, multiplier = 1.6f, diamondCost = 25 };
        [SerializeField] private BoostConfig jumpBoost = new BoostConfig { duration = 15f, multiplier = 1.5f, diamondCost = 25 };
        [SerializeField] private BoostConfig visionBoost = new BoostConfig { duration = 15f, multiplier = 1f, diamondCost = 25 };

        [Header("Vision Boost Settings")]
        [SerializeField] private Color visionOutlineColor = Color.yellow;
        [SerializeField] private float visionOutlineWidth = 4f;

        public Color VisionOutlineColor => visionOutlineColor;
        public float VisionOutlineWidth => visionOutlineWidth;

        [SerializeField] private PlayerController _player;

        /// <summary>Fired whenever a boost starts or ends (use for showing/hiding the timer window).</summary>
        public event Action OnBoostsChanged;

        /// <summary>Fired whenever a boost stock count changes.</summary>
        public event Action<BoostStockEventArgs> OnStockChanged;

        private readonly Dictionary<BoostType, float> _remaining = new Dictionary<BoostType, float>();
        private readonly Dictionary<BoostType, float> _peak = new Dictionary<BoostType, float>();
        private readonly Dictionary<BoostType, int> _stock = new Dictionary<BoostType, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private BoostConfig ConfigFor(BoostType type)
        {
            if (type == BoostType.Speed) return speedBoost;
            if (type == BoostType.Jump) return jumpBoost;
            return visionBoost;
        }

        #region Public API

        public bool IsActive(BoostType type) => _remaining.TryGetValue(type, out var t) && t > 0f;

        public float GetRemaining(BoostType type) => _remaining.TryGetValue(type, out var t) ? Mathf.Max(0f, t) : 0f;

        public float GetFraction(BoostType type)
        {
            if (!IsActive(type)) return 0f;
            float peak = _peak.TryGetValue(type, out var p) ? p : 0f;
            return peak > 0f ? Mathf.Clamp01(GetRemaining(type) / peak) : 0f;
        }

        public bool AnyActive() => IsActive(BoostType.Speed) || IsActive(BoostType.Jump) || IsActive(BoostType.Vision);

        public int GetCost(BoostType type) => ConfigFor(type).diamondCost;
        public Sprite GetIcon()
        {
            var database = CurrencyService.Instance.Database;
            if (database != null)
                return database.GetDefinition(CurrencyType.Diamond).Icon;
            else
                return null;
        }

        /// <summary>Returns current stock count for the given boost type.</summary>
        public int GetStock(BoostType type)
        {
            return _stock.TryGetValue(type, out var s) ? s : 0;
        }

        /// <summary>Adds stock (e.g. from rewarded ad).</summary>
        public void AddStock(BoostType type, int amount = 1)
        {
            int current = GetStock(type);
            _stock[type] = current + amount;
            OnStockChanged?.Invoke(new BoostStockEventArgs(type, _stock[type]));
            Debug.Log($"[Boost] {type} stock: {current} -> {_stock[type]}");
        }

        /// <summary>Tries to spend 1 stock. Returns true if stock was available and spent.</summary>
        private bool TrySpendStock(BoostType type)
        {
            int current = GetStock(type);
            if (current <= 0) return false;

            _stock[type] = current - 1;
            OnStockChanged?.Invoke(new BoostStockEventArgs(type, _stock[type]));
            Debug.Log($"[Boost] {type} stock spent: {current} -> {_stock[type]}");
            return true;
        }

        public void RequestBoost(BoostType type)
        {
            // 1. Try to use accumulated stock first.
            if (TrySpendStock(type))
            {
                Debug.Log($"[Boost] {type} activated from stock.");
                Activate(type);
                return;
            }

            // 2. No stock - try to pay with crystals (Diamond).
            // var currency = CurrencyService.Instance;
            // if (currency != null && currency.TrySpend(CurrencyType.Diamond, config.diamondCost))
            // {
            //     Debug.Log($"[Boost] {type} purchased for {config.diamondCost} crystals.");
            //     Activate(type);
            //     return;
            // }

            // 3. Not enough crystals >> fall back to a Reward ad to grant stock.
             Debug.Log($"[Boost] No stock for {type}. Offering reward ad...");
             var ads = AppServices.Ads;
             if (ads != null && ads.IsRewardedAdReady())
             {
                 ads.ShowRewardedAd(
                     onRewarded: () =>
                     {
                        Debug.Log($"[Boost] Reward granted -> adding 1 stock for {type}.");
                        AddStock(type, 1);
                     },
                     onClosed: null);
             }
            else
             {
                Debug.LogWarning($"[Boost] {type} unavailable: no stock and no reward ad ready.");
                 ads?.LoadRewardedAd();
             }
        }

        #endregion

        private void Activate(BoostType type)
        {
            var config = ConfigFor(type);
            bool wasActive = IsActive(type);

            float newRemaining = GetRemaining(type) + config.duration; // stack
            _remaining[type] = newRemaining;
            _peak[type] = Mathf.Max(_peak.TryGetValue(type, out var p) ? p : 0f, newRemaining);
            if(!config.isActived)
            {
                ApplyMultiplier(type, config.multiplier);
                config.isActived = true;
            }

            if (!wasActive)
                OnBoostsChanged?.Invoke();
        }

        private void ApplyMultiplier(BoostType type, float multiplier)
        {
            if (type == BoostType.Vision)
            {
                SetVisionBoostActive(true);
                return;
            }

            var player = _player;
            if (player == null)
            {
                Debug.LogWarning("[Boost] PlayerController not found; cannot apply boost.");
                return;
            }
            if (type == BoostType.Speed)
                player.ApplySpeedBoost(multiplier);
            else if (type == BoostType.Jump)
                player.AddJumpMultiplier(multiplier);
        }

        private void ResetMultiplier(BoostType type, float multiplier)
        {
            ConfigFor(type).isActived = false;

            if (type == BoostType.Vision)
            {
                SetVisionBoostActive(false);
                return;
            }

            var player = _player;
            if (player == null) return;

            if (type == BoostType.Speed)
                player.RemoveSpeedMultiplayer(multiplier);
            else if (type == BoostType.Jump)
                player.RemoveJumpMultiplayer(multiplier);
        }

        private void Update()
        {
            TickBoost(BoostType.Speed);
            TickBoost(BoostType.Jump);
            TickBoost(BoostType.Vision);
        }

        private void SetVisionBoostActive(bool active)
        {
            var brainrots = UnityEngine.Object.FindObjectsByType<Pickups.BrainrotMapInstance>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var brainrot in brainrots)
            {
                if (brainrot != null)
                {
                    brainrot.SetOutline(active, visionOutlineColor, visionOutlineWidth);
                }
            }
        }

        private void TickBoost(BoostType type)
        {
            if (!IsActive(type)) return;

            _remaining[type] -= Time.deltaTime;
            if (_remaining[type] <= 0f)
            {
                _remaining[type] = 0f;
                _peak[type] = 0f;
                ResetMultiplier(type, ConfigFor(type).multiplier);
                OnBoostsChanged?.Invoke();
            }
        }
    }
}