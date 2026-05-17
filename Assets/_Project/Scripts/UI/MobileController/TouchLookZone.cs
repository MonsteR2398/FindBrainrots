using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class TouchLookZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public CinemachineCamera vcam;
    public float sensitivity = 0.1f;
    
    private CinemachineOrbitalFollow orbitalFollow;

    private void Start()
    {
        if (vcam == null) vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null) orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (orbitalFollow == null) return;

        orbitalFollow.HorizontalAxis.Value += eventData.delta.x * sensitivity;
        orbitalFollow.VerticalAxis.Value -= eventData.delta.y * sensitivity;
        
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(orbitalFollow.VerticalAxis.Value, -20f, 70f);
    }

    public void OnPointerUp(PointerEventData eventData) { }
}


