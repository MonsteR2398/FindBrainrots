using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasures.CurrencySystem
{
    [Serializable]
    public class CurrencyTargetMapping
    {
        public CurrencyDefinition definition;
        public RectTransform targetTransform;
    }

    public class CurrencyFlyVFX : MonoBehaviour
    {
        [SerializeField] private SimpleObjectPool pool;
        [SerializeField] private List<CurrencyTargetMapping> targets;
        [SerializeField] private int maxItemsPerReward = 10;

        public event Action<CurrencyType, long> OnItemReachedTarget; // currencyId, value

        private Dictionary<CurrencyType, RectTransform> _targetLookup = new Dictionary<CurrencyType, RectTransform>();

        private void Awake()
        {
            foreach (var mapping in targets)
            {
                if (mapping.definition != null)
                    _targetLookup[mapping.definition.Type] = mapping.targetTransform;
            }
        }

        public void SpawnVFX(CurrencyType currencyType, long totalAmount, Vector3 spawnPosition)
        {
            if (!_targetLookup.ContainsKey(currencyType))
            {
                Debug.LogWarning($"No target mapping for currency: {currencyType}");
                OnItemReachedTarget?.Invoke(currencyType, totalAmount);
                return;
            }

            RectTransform target = _targetLookup[currencyType];
            CurrencyDefinition def = targets.Find(x => x.definition.Type == currencyType).definition;

            int itemsToSpawn = Mathf.Min(maxItemsPerReward, (int)Mathf.Max(1, totalAmount));
            long amountPerItem = totalAmount / itemsToSpawn;
            long remainder = totalAmount % itemsToSpawn;

            for (int i = 0; i < itemsToSpawn; i++)
            {
                long value = amountPerItem + (i == 0 ? remainder : 0);
                GameObject obj = pool.Get();
                CurrencyVFXItem item = obj.GetComponent<CurrencyVFXItem>();
                item.Play(def.Icon, spawnPosition, target, value, (v) => {
                    OnItemReachedTarget?.Invoke(currencyType, v);
                    pool.Release(obj);
                });
            }
        }
    }
}
