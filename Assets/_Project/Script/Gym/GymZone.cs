using UnityEngine;

public enum GymType { Speed, Jump }

public class GymZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public GymType gymType;
    public Transform spawnPoint;
    private PlayerController player;

    public PlayerController PlayerController => player;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.GetComponent<PlayerController>();
        if (player == null) return;
        GymController.instance.SetPlayerAnimator(other.GetComponentInChildren<Animator>());
        GymController.instance.OnPlayerEnterZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.GetComponent<PlayerController>();
        if (player == null) return;
        GymController.instance.OnPlayerExitZone(this);
    }
}
