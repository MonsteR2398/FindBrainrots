using UnityEngine;

public abstract class PlayerEffectZone : MonoBehaviour
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            ApplyEffect(player);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            RemoveEffect(player);
    }

    protected abstract void ApplyEffect(PlayerController player);
    protected virtual void RemoveEffect(PlayerController player) { }
}
