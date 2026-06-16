using System;
using System.Collections.Generic;
using UnityEngine;
using Treasures.CurrencySystem;
using Treasures.Services;

namespace Treasures.Boosts
{
    public enum BoostType
    {
        Speed,
        Jump
    }

    /// <summary>
    /// Central manager for temporary player boosts (speed / jump).
    /// - Activation tries to spend Diamond crystals first; if the player can't afford it,
    ///   it falls back to showing a Reward ad (boost granted only on successful reward).
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

        [SerializeField] private PlayerController _player;

        /// <summary>Fired whenever a boost starts or ends (use for showing/hiding the timer window).</summary>
        public event Action OnBoostsChanged;

        private readonly Dictionary<BoostType, float> _remaining = new Dictionary<BoostType, float>();
        private readonly Dictionary<BoostType, float> _peak = new Dictionary<BoostType, float>();

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

        private BoostConfig ConfigFor(BoostType type) => type == BoostType.Speed ? speedBoost : jumpBoost;

        #region Public API

        public bool IsActive(BoostType type) => _remaining.TryGetValue(type, out var t) && t > 0f;

        public float GetRemaining(BoostType type) => _remaining.TryGetValue(type, out var t) ? Mathf.Max(0f, t) : 0f;

        public float GetFraction(BoostType type)
        {
            if (!IsActive(type)) return 0f;
            float peak = _peak.TryGetValue(type, out var p) ? p : 0f;
            return peak > 0f ? Mathf.Clamp01(GetRemaining(type) / peak) : 0f;
        }

        public bool AnyActive() => IsActive(BoostType.Speed) || IsActive(BoostType.Jump);

        public int GetCost(BoostType type) => ConfigFor(type).diamondCost;
        public Sprite GetIcon()
        {
            var database = CurrencyService.Instance.Database;
            if (database != null)
                return database.GetDefinition(CurrencyType.Diamond).Icon;
            else
                return null;
        }

        public void RequestBoost(BoostType type)
        {
            var config = ConfigFor(type);

            // 1. Try to pay with crystals (Diamond).
            // var currency = CurrencyService.Instance;
            // if (currency != null && currency.TrySpend(CurrencyType.Diamond, config.diamondCost))
            // {
            //     Debug.Log($"[Boost] {type} purchased for {config.diamondCost} crystals.");
            //     Activate(type);
            //     return;
            // }

            // 2. Not enough crystals >> fall back to a Reward ad.
             Debug.Log($"[Boost] Not enough crystals for {type}. Offering reward ad...");
             var ads = AppServices.Ads;
             if (ads != null && ads.IsRewardedAdReady())
             {
                 ads.ShowRewardedAd(
                     onRewarded: () =>
                     {
                        Debug.Log($"[Boost] Reward granted -> activating {type}.");
                        Activate(type);
                     },
                     onClosed: null);
             }
            else
             {
                Debug.LogWarning($"[Boost] {type} unavailable: not enough crystals and no reward ad ready.");
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
            var player = _player;
            if (player == null)
            {
                Debug.LogWarning("[Boost] PlayerController not found; cannot apply boost.");
                return;
            }
            if (type == BoostType.Speed)
                player.ApplySpeedBoost(multiplier);
            else
                player.AddJumpMultiplier(multiplier);
        }

        private void ResetMultiplier(BoostType type, float multiplier)
        {
            var player = _player;
            if (player == null) return;

            ConfigFor(type).isActived = false;

            if (type == BoostType.Speed)
                player.RemoveSpeedMultiplayer(multiplier);
            else
                player.RemoveJumpMultiplayer(multiplier);
        }

        private void Update()
        {
            TickBoost(BoostType.Speed);
            TickBoost(BoostType.Jump);
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
