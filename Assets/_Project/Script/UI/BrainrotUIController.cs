using UnityEngine;
using ModularCollection.Data;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BrainrotUIController : MonoBehaviour
{
    public static BrainrotUIController Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image glitterImage;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Transform objectTarget;
    [SerializeField] private Button skipButton;

    private Animator animator;
    private bool isSpeededUp = false;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

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

        if (animationPanel != null)
        {
            animationPanel.SetActive(false);
            animator = animationPanel.GetComponent<Animator>();
            
            if (skipButton != null)
                skipButton.onClick.AddListener(OnPanelClick);
        }
    }

    private void OnPanelClick()
    {
        if (animator == null)
        {
            HideAnimation();
            return;
        }

        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isFinished = stateInfo.normalizedTime >= 1.0f;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            if (!isFinished && !isSpeededUp)
            {
                animator.speed = 5f;
                isSpeededUp = true;
            }
        }
        if (isFinished)
        {
            HideAnimation();
            animator.speed = 1f;
            isSpeededUp = false;
        }
    }

    public void PlayUnlockAnimation(CollectibleItemSO item)
{
        if (animationPanel == null) return;

        animationPanel.SetActive(true);
        if (animator != null)
        {
            animator.speed = 1f;
            isSpeededUp = false;
            animator.Play(0, 0, 0f);
        }

        if (itemNameText != null) itemNameText.text = item.ItemName;
        if (itemIcon != null) itemIcon.sprite = item.Icon;
        if (glitterImage != null) glitterImage.sprite = item.Rarity.GlitterSprite;
        if (cardImage != null) cardImage.sprite = item.Rarity.CardSprite;
        if (rarityText != null)
        {
            rarityText.text = item.Rarity != null ? item.Rarity.DisplayName : "";
            rarityText.color = item.Rarity != null ? item.Rarity.Color : Color.white;
        }
        if (objectTarget != null)
        {
            foreach (Transform child in objectTarget)
                Destroy(child.gameObject);
            
            GameObject model = Instantiate(item.WorldPrefab, objectTarget);
            SetLayerRecursively(model, 5); // 5 = UI layer
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public void HideAnimation()
{
        if (animationPanel != null) animationPanel.SetActive(false);
    }
}
