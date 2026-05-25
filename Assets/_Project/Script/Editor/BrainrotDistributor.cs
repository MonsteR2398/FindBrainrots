#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ModularCollection.Data;
using Treasures.Pickups;

public class BrainrotDistributor : EditorWindow
{
    [MenuItem("Tools/Brainrot/Distribute Items on Map")]
    public static void Distribute()
    {
        var category = AssetDatabase.FindAssets("t:CollectionCategorySO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CollectionCategorySO>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(c => c.CategoryID.ToLower().Contains("brainrot"));

        if (category == null)
        {
            Debug.LogError("Brainrot Collection Category not found! Ensure you have a CollectionCategorySO with 'brainrot' in its ID.");
            return;
        }

        var sceneInstances = Object.FindObjectsByType<BrainrotMapInstance>(FindObjectsSortMode.None);
        
        if (sceneInstances.Length == 0)
        {
            Debug.LogWarning("No BrainrotMapInstance components found in the scene.");
            return;
        }

        HashSet<string> usedIds = new HashSet<string>();
        int assignedCount = 0;

        var availableItems = category.Items.OrderBy(x => Random.value).ToList();

        foreach (var instance in sceneInstances)
        {
            var selected = availableItems.FirstOrDefault(i => 
                (instance.RarityFilter == null || i.Rarity == instance.RarityFilter) && 
                !usedIds.Contains(i.ItemID));

            if (selected != null)
            {
                instance.SetItem(selected);
                instance.UpdateVisuals();
                usedIds.Add(selected.ItemID);
                assignedCount++;
            }
            else
            {
                string rarityName = instance.RarityFilter != null ? instance.RarityFilter.name : "Any";
                Debug.LogWarning($"No unique items left for rarity '{rarityName}' at {instance.name}");
            }
        }

        Debug.Log($"Successfully distributed {assignedCount} brainrots across the map.");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
#endif
