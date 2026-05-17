using UnityEngine;

public class TrampolineEffect : PlayerEffectZone
{
    public float launchForce = 15f;

    protected override void ApplyEffect(PlayerController player)
    {
        player.Launch(Vector3.up * launchForce);
    }
}
