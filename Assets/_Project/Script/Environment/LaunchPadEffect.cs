using UnityEngine;

public class LaunchPadEffect : PlayerEffectZone
{
    public float launchForce = 20f;
    public Vector3 launchDirection = Vector3.up;
    public bool useRelativeDirection = true;

    // Визуальные настройки для Gizmos
    public Color gizmoColor = Color.green;
    public float gizmoArrowLength = 2f;
    public float gizmoArrowHeadSize = 0.5f;

    protected override void ApplyEffect(PlayerController player)
    {
        Vector3 finalDirection = useRelativeDirection ? transform.TransformDirection(launchDirection) : launchDirection;
        player.Launch(finalDirection.normalized * launchForce);
    }

    private void OnDrawGizmosSelected()
    {
        DrawLaunchGizmo();
    }

    private void OnDrawGizmos()
    {
        DrawLaunchGizmo();
    }

    private void DrawLaunchGizmo()
    {
        Vector3 origin = transform.position;
        Vector3 direction = useRelativeDirection ? transform.TransformDirection(launchDirection.normalized) : launchDirection.normalized;
        float length = gizmoArrowLength;

        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(origin, direction * length);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward;
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(20, 0, 0) * Vector3.forward;
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(-20, 0, 0) * Vector3.forward;

        Gizmos.DrawRay(origin + direction * length, right * gizmoArrowHeadSize);
        Gizmos.DrawRay(origin + direction * length, left * gizmoArrowHeadSize);
        Gizmos.DrawRay(origin + direction * length, up * gizmoArrowHeadSize);
        Gizmos.DrawRay(origin + direction * length, down * gizmoArrowHeadSize);
    }
}