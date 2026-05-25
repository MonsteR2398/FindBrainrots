using System.Collections.Generic;

namespace ModularCollection.Core
{
    public interface ICollectionPersistence
    {
        void SaveUnlockedItems(HashSet<string> unlockedIds);
        HashSet<string> LoadUnlockedItems();
        
        void SaveReadItems(HashSet<string> readIds);
        HashSet<string> LoadReadItems();
    }
}
