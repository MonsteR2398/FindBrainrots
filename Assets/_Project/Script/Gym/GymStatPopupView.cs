using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GymStatPopupView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text _text;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Vector2 _startPosition;

    private float _duration;
    private float _floatUp;
    private float _elapsed;

    private void Update()
    {
        if (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;
            
            // Float up from start position
            _rectTransform.anchoredPosition = _startPosition + Vector2.up * _floatUp * t;
            
            // Fade out
            if (_text != null)
            {
                Color color = _text.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                _text.color = color;
            }
        }
        else
        {
            // Reset to start position before hiding
            _rectTransform.anchoredPosition = _startPosition;
            gameObject.SetActive(false);
        }
    }

    public void Play(float delta, float inTime, float outTime, float floatUp)
    {
        _duration = outTime;
        _floatUp = floatUp;
        _elapsed = 0f;

        // Reset to start position
        _rectTransform.anchoredPosition = _startPosition;

        if (_text != null)
        {
            _text.text = delta.ToString("F1");
            Color color = _text.color;
            color.a = 1f;
            _text.color = color;
        }

        gameObject.SetActive(true);
    }
}
