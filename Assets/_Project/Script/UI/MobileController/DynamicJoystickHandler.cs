using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class DynamicJoystickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    public RectTransform joystickTransform;
    public OnScreenStick stick;
    public CanvasGroup joystickCanvasGroup;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

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

        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        RectTransform parentRect = joystickTransform.parent as RectTransform;
        if (parentRect != null)
        {
            // Use the camera that triggered the event, or fallback to canvas camera
            Camera uiCamera = eventData.pressEventCamera;
            if (uiCamera == null && _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = _canvas.worldCamera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, uiCamera, out Vector2 localPoint))
            {
                joystickTransform.anchoredPosition = localPoint;
               // Debug.Log($"Joystick moved to local: {localPoint} (Screen: {eventData.position}, Camera: {(uiCamera != null ? uiCamera.name : "null")})");
            }
            else
            {
                Debug.LogWarning("Failed to convert screen point to local point in rectangle.");
            }
        }
        else
        {
            Debug.LogError("joystickTransform.parent is not a RectTransform!");
        }
        
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


