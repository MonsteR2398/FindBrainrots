using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraLookController : MonoBehaviour
{
    [Header("Settings")]
    public float mouseSensitivity = 0.1f;
    public float touchSensitivity = 0.1f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 10f;
    public float currentZoomDistance = 5f;

    [Header("References")]
    public CinemachineCamera vcam;
    
    private CinemachineOrbitalFollow orbitalFollow;
    private InputAction lookAction;

    private void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();
        if (vcam != null) 
        {
            orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null)
            {
                currentZoomDistance = orbitalFollow.Radius;
            }
        }

        lookAction = InputSystem.actions.FindAction("Look");

        // The cursor is always visible and never locked in this game.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (orbitalFollow == null || lookAction == null) return;

        if (IsAnyWindowOpen()) return;

        // Safety net: keep the cursor visible/unlocked no matter what
        // (ad pause restore, other systems, browser focus quirks).
        if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // On mobile/touchscreen devices, touch camera rotation is handled exclusively by TouchLookZone.
        // Therefore, we ignore touch inputs in this controller to prevent double-input and joystick conflict.
        if (IsTouchInput()) return;

        // Rotate with mouse delta whenever the pointer is not over UI.
        if (IsPointerOverUI()) return;

        Vector2 delta = lookAction.ReadValue<Vector2>();

        if (delta.sqrMagnitude > 0)
        {
            float baseSensitivity = mouseSensitivity;
            float currentSensitivity = baseSensitivity * Treasures.Settings.GameSettings.SensitivityMultiplier;
            
            orbitalFollow.HorizontalAxis.Value += delta.x * currentSensitivity;
            orbitalFollow.VerticalAxis.Value -= delta.y * currentSensitivity;
            
            orbitalFollow.VerticalAxis.Value = Mathf.Clamp(orbitalFollow.VerticalAxis.Value, -20f, 70f);
        }
    }

    private bool IsTouchInput()
    {
        return Touchscreen.current != null && Touchscreen.current.touches.Count > 0;
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public void ForceZoomTo(float distance)
    {
        if (orbitalFollow != null)
        {
            currentZoomDistance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);
            orbitalFollow.Radius = currentZoomDistance;
        }
    }

    public float saveZoom
    {
        get { return currentZoomDistance; }
    }

    private bool IsAnyWindowOpen()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return false;

        string[] windowNames = { "SkinShopUI", "CollectionUI", "AchievementUI", "SettingsUI", "PortalWindowUI", "IAPShopUI", "TemporaryOfferPopup", "FindBrainrotUI" };
        foreach (string name in windowNames)
        {
            Transform t = canvas.transform.Find(name);
            if (t != null && t.gameObject.activeInHierarchy)
            {
                if (t.childCount > 0 && t.GetChild(0).gameObject.activeSelf)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
