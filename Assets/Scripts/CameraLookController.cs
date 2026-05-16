using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraLookController : MonoBehaviour
{
    [Header("Settings")]
    public float mouseSensitivity = 0.1f;
    public float touchSensitivity = 0.1f;
    public bool lockCursorOnStart = true;

    [Header("References")]
    public CinemachineCamera vcam;
    
    private CinemachineOrbitalFollow orbitalFollow;
    private InputAction lookAction;

    private void Start()
    {
        if (vcam == null) vcam = GetComponent<CinemachineCamera>();
        if (vcam != null) orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();

        lookAction = InputSystem.actions.FindAction("Look");

        if (lockCursorOnStart && IsPointerOverUI() == false)
        {
            LockCursor();
        }
    }

    private void Update()
    {
        if (orbitalFollow == null || lookAction == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            LockCursor();
        }

        Vector2 delta = lookAction.ReadValue<Vector2>();

        if (delta.sqrMagnitude > 0)
        {
            float currentSensitivity = IsTouchInput() ? touchSensitivity : mouseSensitivity;
            
            if (Cursor.lockState == CursorLockMode.Locked || IsTouchInput())
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
}
