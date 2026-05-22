using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Treasures.CurrencySystem
{
    public class CurrencyVFXItem : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private AnimationCurve explosionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField] private AnimationCurve flyCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Play(Sprite icon, Vector3 startPos, RectTransform target, long value, Action<long> onReached)
        {
            iconImage.sprite = icon;
            _rectTransform.position = startPos;
            StartCoroutine(Animate(target, value, onReached));
        }

        private IEnumerator Animate(RectTransform target, long value, Action<long> onReached)
        {
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * 150f;
            randomDir.z = 0;
            Vector3 explosionEnd = _rectTransform.localPosition + randomDir;
            Vector3 explosionStart = _rectTransform.localPosition;

            float duration = 0.4f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _rectTransform.localPosition = Vector3.Lerp(explosionStart, explosionEnd, explosionCurve.Evaluate(t));
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            elapsed = 0;
            duration = 0.6f;
            Vector3 flyStart = _rectTransform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                Vector3 targetPos = target.position;
                Vector3 currentPos = Vector3.Lerp(flyStart, targetPos, flyCurve.Evaluate(t));
                
                float arcHeight = 100f * Mathf.Sin(t * Mathf.PI);
                currentPos.y += arcHeight;

                _rectTransform.position = currentPos;
                yield return null;
            }

            onReached?.Invoke(value);
            gameObject.SetActive(false);
        }
    }
}
