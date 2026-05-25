using System.Collections.Generic;
using UnityEngine;

namespace ModularCollection.Data
{
    [CreateAssetMenu(fileName = "RarityDatabase", menuName = "Modular Collection/Rarity Database")]
    public class RarityDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<RaritySettingsSO> rarities = new List<RaritySettingsSO>();

        public IReadOnlyList<RaritySettingsSO> Rarities => rarities;

        public RaritySettingsSO GetRarityByID(string id)
        {
            return rarities.Find(r => r.RarityID == id);
        }

        public void Initialize(List<RaritySettingsSO> list)
        {
            rarities = list;
        }
    }
}
