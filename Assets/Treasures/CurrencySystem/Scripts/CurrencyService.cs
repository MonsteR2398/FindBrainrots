using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public class CurrencyService : MonoBehaviour
    {
        public static CurrencyService Instance { get; private set; }

        [SerializeField] private CurrencyDatabaseSO database;
        public CurrencyDatabaseSO Database => database;

        public event Action<CurrencyType, long, bool> OnBalanceChanged; // type, newAmount, isSilent
        public event Action<CurrencyType, long> OnCurrencyReceived; // type, addedAmount (for VFX)

        private Dictionary<CurrencyType, long> _balances = new Dictionary<CurrencyType, long>();
        private string _savePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);

            _savePath = Path.Combine(Application.persistentDataPath, "wallet.json");
            Load();
        }

        public long GetBalance(CurrencyType type)
        {
            return _balances.ContainsKey(type) ? _balances[type] : 0;
        }

        public void AddBalance(CurrencyType type, long amount, bool silent = false)
        {
            if (amount == 0) return;

            if (!_balances.ContainsKey(type))
                _balances[type] = 0;

            _balances[type] += amount;
            
            Save();

            OnBalanceChanged?.Invoke(type, _balances[type], silent);

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
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(_savePath, json);
        }

        private void Load()
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                WalletData data = JsonUtility.FromJson<WalletData>(json);
                _balances.Clear();
                foreach (var pair in data.balances)
                {
                    _balances[pair.type] = pair.amount;
                }
            }
        }
    }
}
