using UnityEngine;
using ModularCollection.Core;
using ModularCollection.Data;
using ModularTreasures.Achievements;

namespace Treasures.Pickups
{
    public class BrainrotMapInstance : MonoBehaviour
    {
        [Header("Data (Set by Distributor)")]
        [SerializeField] private CollectibleItemSO itemData;
        
        [Header("Settings")]
        [SerializeField] private RaritySettingsSO rarityFilter;
        [SerializeField] private string achievementId = "brainrots_found";
        
        [Header("Visuals")]
        [SerializeField] private Transform modelContainer;
        [SerializeField] private Material placeholderMaterial;

        public RaritySettingsSO RarityFilter => rarityFilter;

        private void Start()
        {
            if (itemData == null || CollectionManager.Instance.IsUnlocked(itemData.ItemID))
            {
                gameObject.SetActive(false);
                return;
            }

            UpdateVisuals();
        }

        public void SetItem(CollectibleItemSO data)
        {
            itemData = data;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void UpdateVisuals()
        {
            if (itemData == null || itemData.WorldPrefab == null || modelContainer == null) return;

            // Clear existing models
            foreach (Transform child in modelContainer)
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }

            // Instantiate new model
            GameObject visual = Instantiate(itemData.WorldPrefab, modelContainer);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (placeholderMaterial != null)
                {
                    // Apply black placeholder material to all slots
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++) mats[i] = placeholderMaterial;
                    r.sharedMaterials = mats;
                }
                else if (itemData.OverrideTexture != null)
                {
                    // Apply rarity texture using sharedMaterials to avoid leaks in Editor
                    Material[] sharedMats = r.sharedMaterials;
                    foreach (var mat in sharedMats)
                    {
                        if (mat.HasProperty("_BaseMap"))
                            mat.SetTexture("_BaseMap", itemData.OverrideTexture);
                        else if (mat.HasProperty("_MainTex"))
                            mat.SetTexture("_MainTex", itemData.OverrideTexture);
                    }
                }
            }
        }

        public void ProcessPickup()
        {
            if (itemData == null) return;

            CollectionManager.Instance.UnlockItem(itemData.ItemID);

            if (AchievementManager.Instance != null)
                AchievementManager.Instance.AddProgress(achievementId, 1);

            if (BrainrotUIController.Instance != null)
                BrainrotUIController.Instance.PlayUnlockAnimation(itemData);
            
            Debug.Log($"[Brainrot] Collected: {itemData.ItemName}");
        }
    }
}
