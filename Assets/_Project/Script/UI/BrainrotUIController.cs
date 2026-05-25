using UnityEngine;
using ModularCollection.Data;
using TMPro;
using UnityEngine.UI;

public class BrainrotUIController : MonoBehaviour
{
    public static BrainrotUIController Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI rarityText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (animationPanel != null) animationPanel.SetActive(false);
    }

    public void PlayUnlockAnimation(CollectibleItemSO item)
    {
        if (animationPanel == null) return;

        animationPanel.SetActive(true);
        
        if (itemNameText != null) itemNameText.text = item.ItemName;
        if (itemIcon != null) itemIcon.sprite = item.Icon;
        if (rarityText != null) 
        {
            rarityText.text = item.Rarity != null ? item.Rarity.DisplayName : "";
            rarityText.color = item.Rarity != null ? item.Rarity.RarityColor : Color.white;
        }

        // Here you can trigger an actual Animator or Tween
        Debug.Log($"[UI] Playing animation for {item.ItemName}");
        
        // For now, let's just keep it simple or auto-hide after some time
        // Invoke(nameof(HideAnimation), 3f); 
    }

    public void HideAnimation()
    {
        if (animationPanel != null) animationPanel.SetActive(false);
    }
}
