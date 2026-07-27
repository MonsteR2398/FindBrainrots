using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraLookController : MonoBehaviour
{
    [Header("Settings")]
    public float mouseSensitivity = 0.1f;
    public float touchSensitivity = 0.1f;
    public bool lockCursorOnStart = true;
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

        if (lockCursorOnStart && IsPointerOverUI() == false)
        {
            LockCursor();
        }
    }

    private void Update()
    {
        if (orbitalFollow == null || lookAction == null) return;

        if (IsAnyWindowOpen())
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                UnlockCursor();
            }
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            LockCursor();
        }

        // On mobile/touchscreen devices, touch camera rotation is handled exclusively by TouchLookZone.
        // Therefore, we ignore touch inputs in this controller to prevent double-input and joystick conflict.
        if (IsTouchInput()) return;

        Vector2 delta = lookAction.ReadValue<Vector2>();

        if (delta.sqrMagnitude > 0)
        {
            float baseSensitivity = mouseSensitivity;
            float currentSensitivity = baseSensitivity * Treasures.Settings.GameSettings.SensitivityMultiplier;
            
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                orbitalFollow.HorizontalAxis.Value += delta.x * currentSensitivity;
                orbitalFollow.VerticalAxis.Value -= delta.y * currentSensitivity;
                
                orbitalFollow.VerticalAxis.Value = Mathf.Clamp(orbitalFollow.VerticalAxis.Value, -20f, 70f);
            }
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
