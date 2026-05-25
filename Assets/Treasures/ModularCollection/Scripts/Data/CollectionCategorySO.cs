using System.Collections.Generic;
using UnityEngine;

namespace ModularCollection.Data
{
    [CreateAssetMenu(fileName = "CollectionCategory", menuName = "Modular Collection/Collection Category")]
    public class CollectionCategorySO : ScriptableObject
    {
        [SerializeField] private string categoryID;
        [SerializeField] private string displayName;
        [SerializeField] private List<CollectibleItemSO> items = new List<CollectibleItemSO>();

        public string CategoryID => categoryID;
        public string DisplayName => displayName;
        public IReadOnlyList<CollectibleItemSO> Items => items;

        public void Initialize(string id, string name, List<CollectibleItemSO> items)
        {
            this.categoryID = id;
            this.displayName = name;
            this.items = items;
        }
    }
}
