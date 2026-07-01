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
        if (IsAnyWindowOpen()) return;

        float currentSensitivity = sensitivity * Treasures.Settings.GameSettings.SensitivityMultiplier;

        orbitalFollow.HorizontalAxis.Value += eventData.delta.x * currentSensitivity;
        orbitalFollow.VerticalAxis.Value -= eventData.delta.y * currentSensitivity;
        
        orbitalFollow.VerticalAxis.Value = Mathf.Clamp(orbitalFollow.VerticalAxis.Value, -20f, 70f);
    }

    public void OnPointerUp(PointerEventData eventData) { }

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


