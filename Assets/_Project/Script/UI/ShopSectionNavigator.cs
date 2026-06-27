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
            [Tooltip("Значение anchoredPosition.y контента, до которого прокручивать.")]
            public float targetScrollY;
        }

        [Header("References")]
        [SerializeField] private ScrollRect scrollRect;

        [Header("Mappings")]
        [SerializeField] private List<SectionMapping> mappings;

        [Header("Settings")]
        [SerializeField] private float scrollDuration = 0.3f;

        private Coroutine scrollCoroutine;

        private void Awake()
        {
            foreach (var mapping in mappings)
            {
                if (mapping.sectionButton != null)
                {
                    var capturedScrollY = mapping.targetScrollY;
                    mapping.sectionButton.onClick.AddListener(() => ScrollTo(capturedScrollY));
                }
            }
        }

        public void ScrollTo(float targetScrollY)
        {
            if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
            scrollCoroutine = StartCoroutine(ScrollToCoroutine(targetScrollY));
        }

        private IEnumerator ScrollToCoroutine(float targetScrollY)
        {
            Vector2 startPosition = scrollRect.content.anchoredPosition;
            Vector2 targetPosition = new Vector2(scrollRect.content.anchoredPosition.x, targetScrollY);

            float elapsed = 0;
            while (elapsed < scrollDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scrollDuration;
                t = t * t * (3f - 2f * t);
                scrollRect.content.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            scrollRect.content.anchoredPosition = targetPosition;
            scrollCoroutine = null;
        }
    }
}