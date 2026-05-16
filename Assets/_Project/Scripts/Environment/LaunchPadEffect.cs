using UnityEngine;

public class LaunchPadEffect : PlayerEffectZone
{
    public float launchForce = 20f;
    public Vector3 launchDirection = Vector3.up;
    public bool useRelativeDirection = true;

    protected override void ApplyEffect(PlayerController player)
    {
        Vector3 finalDirection = useRelativeDirection ? transform.TransformDirection(launchDirection) : launchDirection;
        player.Launch(finalDirection.normalized * launchForce);
    }
}
