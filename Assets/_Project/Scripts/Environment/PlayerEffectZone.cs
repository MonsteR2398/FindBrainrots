using UnityEngine;

public abstract class PlayerEffectZone : MonoBehaviour
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            Debug.Log($"[EffectZone] Player Entered: {gameObject.name}");
            ApplyEffect(player);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            Debug.Log($"[EffectZone] Player Exited: {gameObject.name}");
            RemoveEffect(player);
        }
    }

    protected abstract void ApplyEffect(PlayerController player);
    protected virtual void RemoveEffect(PlayerController player) { }
}
