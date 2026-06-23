using UnityEngine;
using Treasures.Bots;

public class WaterZone : PlayerEffectZone
{
    protected override void ApplyEffect(PlayerController player)
    {
        if (player.TryGetComponent<BotBrain>(out var brain))
        {
            brain.RecoverToClosestNode();
        }
        else
        {
            player.RespawnAtLastSafePosition();
        }
    }
}