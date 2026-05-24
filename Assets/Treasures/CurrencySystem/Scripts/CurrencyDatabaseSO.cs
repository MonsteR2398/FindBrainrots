using System.Collections.Generic;
using UnityEngine;

namespace Treasures.CurrencySystem
{
    [CreateAssetMenu(fileName = "CurrencyDatabase", menuName = "Treasures/Currency/Database")]
    public class CurrencyDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<CurrencyDefinition> definitions;

        private Dictionary<CurrencyType, CurrencyDefinition> _lookup;

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            _lookup = new Dictionary<CurrencyType, CurrencyDefinition>();
            if (definitions == null) return;
            
            foreach (var def in definitions)
            {
                if (def != null && !_lookup.ContainsKey(def.Type))
                {
                    _lookup.Add(def.Type, def);
                }
            }
        }

        public CurrencyDefinition GetDefinition(CurrencyType type)
        {
            if (_lookup == null) Initialize();
            
            if (_lookup.TryGetValue(type, out var def))
            {
                return def;
            }
            return null;
        }
    }
}
