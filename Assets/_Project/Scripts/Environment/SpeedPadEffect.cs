using UnityEngine;

public class SpeedPadEffect : PlayerEffectZone
{
    public float speedMultiplier = 2.5f;

    protected override void ApplyEffect(PlayerController player)
    {
        // Use ApplySpeedBoost for an instant jump in speed
        player.ApplySpeedBoost(speedMultiplier);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        // Return to normal speed
        player.SetSpeedMultiplier(1f);
    }
}
