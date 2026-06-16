using UnityEngine;

public class SpeedPadEffect : PlayerEffectZone
{
    public float speedMultiplier = 2.5f;

    protected override void ApplyEffect(PlayerController player)
    {
        player.ApplySpeedBoost(speedMultiplier);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        player.RemoveSpeedMultiplayer(speedMultiplier);
    }
}
