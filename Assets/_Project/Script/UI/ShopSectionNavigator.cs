using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Treasures.UI
{
    public class ShopSectionNavigator : MonoBehaviour
    {
        [System.Serializable]
        public struct SectionMapping
        {
            public Button sectionButton;
            public RectTransform targetPanel;
        }

        [Header("References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private List<SectionMapping> mappings;

        [Header("Settings")]
        [SerializeField] private float scrollDuration = 0.3f;

        private Coroutine scrollCoroutine;

        private void Awake()
        {
            foreach (var mapping in mappings)
            {
                if (mapping.sectionButton != null && mapping.targetPanel != null)
                {
                    mapping.sectionButton.onClick.AddListener(() => ScrollToPanel(mapping.targetPanel));
                }
            }
        }

        public void ScrollToPanel(RectTransform target)
        {
            if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
            scrollCoroutine = StartCoroutine(ScrollToCoroutine(target));
        }

        private IEnumerator ScrollToCoroutine(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();

            Vector2 targetPosition = CalculateTargetPosition(target);
            Vector2 startPosition = scrollRect.content.anchoredPosition;

            float elapsed = 0;
            while (elapsed < scrollDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scrollDuration;
                // Smooth step
                t = t * t * (3f - 2f * t);
                scrollRect.content.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            scrollRect.content.anchoredPosition = targetPosition;
            scrollCoroutine = null;
        }

        private Vector2 CalculateTargetPosition(RectTransform target)
        {
            // The content position is relative to its anchor/pivot. 
            // In a VerticalLayoutGroup, it usually grows downwards.
            // We want to move the content so that the 'target' is at the top of the viewport.
            
            float contentHeight = scrollRect.content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            
            // Local position of the target relative to the content's pivot
            float targetLocalY = target.anchoredPosition.y;
            
            // The content's anchoredPosition.y is how much it's scrolled up.
            // If content pivot is at top (1), then anchoredPosition.y = 0 means top is at top.
            // If target is at y = -100, we need anchoredPosition.y = 100 to bring it to top.
            float desiredScrollY = -targetLocalY;
            
            float maxScroll = Mathf.Max(0, contentHeight - viewportHeight);
            desiredScrollY = Mathf.Clamp(desiredScrollY, 0, maxScroll);
            
            return new Vector2(scrollRect.content.anchoredPosition.x, desiredScrollY);
        }
    }
}
