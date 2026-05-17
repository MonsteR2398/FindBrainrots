using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class DynamicJoystickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    public RectTransform joystickTransform;
    public OnScreenStick stick;
    public CanvasGroup joystickCanvasGroup;

    private void Start()
    {
        if (joystickCanvasGroup != null)
        {
            joystickCanvasGroup.alpha = 0f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (joystickTransform == null || stick == null) return;

        joystickTransform.position = eventData.position;
        
        ((RectTransform)stick.transform).anchoredPosition = Vector2.zero;
        
        if (joystickCanvasGroup != null)
            joystickCanvasGroup.alpha = 1f;

        Canvas.ForceUpdateCanvases();
        
        stick.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (stick != null)
            stick.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (stick != null)
        {
            stick.OnPointerUp(eventData);
            ((RectTransform)stick.transform).anchoredPosition = Vector2.zero;
        }

        if (joystickCanvasGroup != null)
            joystickCanvasGroup.alpha = 0f;
    }
}


