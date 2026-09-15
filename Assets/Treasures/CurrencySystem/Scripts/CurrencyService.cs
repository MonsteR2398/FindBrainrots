using System;
using System.Collections.Generic;
using System.IO;
using ModularTreasures.Quests;
using Treasures.Services;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace Treasures.CurrencySystem
{
    [Serializable]
    public class CurrencyPair
    {
        public CurrencyType type;
        public long amount;
    }

    [Serializable]
    public class WalletData
    {
        public List<CurrencyPair> balances = new List<CurrencyPair>();
    }

    /// <summary>
    /// Holds every currency balance of the player. The wallet is persisted through the
    /// PluginYourGames "Storage" module: it is written into the cloud save (<c>YG2.saves</c>) via the
    /// plugin's PlayerPrefs override instead of the device-local file used by earlier builds.
    /// </summary>
    public class CurrencyService : MonoBehaviour
    {
        public static CurrencyService Instance { get; private set; }

        /// <summary>Cloud save key that stores the serialized <see cref="WalletData"/>.</summary>
        public const string WalletKey = "currency.wallet";

        /// <summary>Device-local file name used before the cloud save migration.</summary>
        private const string LegacyFileName = "wallet.json";

        private static bool _legacyFileChecked;

        [SerializeField] private CurrencyDatabaseSO database;
        [SerializeField] private CurrencyFlyVFX vfxManager;
        public CurrencyDatabaseSO Database => database;

        public event Action<CurrencyType, long, bool> OnBalanceChanged; // type, newAmount, isSilent
        public event Action<CurrencyType, long> OnCurrencyReceived; // type, addedAmount (for VFX)

        private Dictionary<CurrencyType, long> _balances = new Dictionary<CurrencyType, long>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);

            Load();

            // The cloud save is applied asynchronously - re-read the wallet when it arrives,
            // otherwise the next AddBalance/TrySpend would push the pre-load wallet to the cloud.
            CloudSaves.Subscribe(OnCloudDataReady);
        }

        private void OnDestroy()
        {
            CloudSaves.Unsubscribe(OnCloudDataReady);
        }

        /// <summary>
        /// Re-syncs the in-memory balances with the cloud save and notifies the UI (silently, so no
        /// VFX are spawned for a reload).
        /// </summary>
        private void OnCloudDataReady()
        {
            Load();

            foreach (var kvp in _balances)
            {
                OnBalanceChanged?.Invoke(kvp.Key, kvp.Value, true);
            }
        }

        public long GetBalance(CurrencyType type)
        {
            return _balances.ContainsKey(type) ? _balances[type] : 0;
        }

        public void AddBalance(CurrencyType type, long amount, Transform targetPos = null,  bool silent = false)
        {
            if (amount == 0) return;

            if (!_balances.ContainsKey(type))
                _balances[type] = 0;

            _balances[type] += amount;
            QuestActionSystem.TriggerAction($"{type}Add", (float)amount);
            Save();

            bool isEffectivelySilent = silent || targetPos == null;
            OnBalanceChanged?.Invoke(type, _balances[type], isEffectivelySilent);

            if (targetPos != null)
            {
                vfxManager.SpawnVFX(type, amount, targetPos.position);
            }

            if (!silent && amount > 0)
            {
                OnCurrencyReceived?.Invoke(type, amount);
            }
        }

        public bool TrySpend(CurrencyType type, long amount)
        {
            if (GetBalance(type) < amount) return false;

            _balances[type] -= amount;
            Save();
            OnBalanceChanged?.Invoke(type, _balances[type], true);
            return true;
        }

        private void Save()
        {
            WalletData data = new WalletData();
            foreach (var kvp in _balances)
            {
                data.balances.Add(new CurrencyPair { type = kvp.Key, amount = kvp.Value });
            }

            // Stored through the YG2 Storage module (cloud save).
            PlayerPrefs.SetString(WalletKey, JsonUtility.ToJson(data));
            CloudSaves.Save();
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(WalletKey, string.Empty);

            if (string.IsNullOrEmpty(json))
                json = ImportLegacyWallet();

            _balances.Clear();

            if (string.IsNullOrEmpty(json)) return;

            try
            {
                WalletData data = JsonUtility.FromJson<WalletData>(json);
                if (data?.balances == null) return;

                foreach (var pair in data.balances)
                {
                    _balances[pair.type] = pair.amount;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CurrencyService] Failed to read the wallet from the cloud save: {e.Message}");
            }
        }

        /// <summary>
        /// One-time import of the device-local <c>wallet.json</c> written by builds that predate the
        /// cloud save migration. The file is removed afterwards so that resetting the cloud save
        /// cannot resurrect the old wallet.
        /// </summary>
        private static string ImportLegacyWallet()
        {
            if (_legacyFileChecked) return string.Empty;
            _legacyFileChecked = true;

            try
            {
                string path = Path.Combine(Application.persistentDataPath, LegacyFileName);
                if (!File.Exists(path)) return string.Empty;

                string json = File.ReadAllText(path);
                File.Delete(path);

                Debug.Log($"[CurrencyService] Migrated the local {LegacyFileName} into the YG2 cloud save.");
                return json;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CurrencyService] Could not migrate the local {LegacyFileName}: {e.Message}");
                return string.Empty;
            }
        }
    }
}
