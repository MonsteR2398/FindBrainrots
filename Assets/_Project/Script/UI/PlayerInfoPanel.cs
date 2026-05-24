using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInfoPanel : MonoBehaviour
{
    private PlayerController player;

    [Header("Display Settings")]
    public Color panelColor = new Color(0, 0, 0, 0.6f);
    public Color textColor = Color.white;
    public int fontSize = 14;
    public float panelWidth = 320f;
    public float panelHeight = 240f;
    public float margin = 10f;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    private void OnGUI()
    {
        if (player == null) return;

        float x = margin;
        float y = margin;
        float lineHeight = fontSize + 4;

        GUI.color = panelColor;
        GUI.Box(new Rect(x, y, panelWidth, panelHeight), "");
        GUI.color = Color.white;

        x += 8;
        y += 8;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        style.alignment = TextAnchor.UpperLeft;

        Vector3 velocity = player.CurrentVelocity;
        float speed = player.CurrentSpeed;
        float maxSpeed = player.MaxSpeed;
        float hSpeed = player.HorizontalSpeed;
        float vSpeed = player.VerticalSpeed;
        float fallSpeed = player.FallSpeed;
        bool grounded = player.IsGrounded;
        bool falling = player.IsFalling;
        bool jumping = player.IsJumping;

        string state;
        if (grounded)
            state = "On Ground";
        else if (jumping)
            state = "Jumping";
        else if (falling)
            state = "Falling";
        else
            state = "Airborne";

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"State: {state}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Speed: {speed:F2} / {maxSpeed:F2}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Horizontal: {hSpeed:F2}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Vertical: {vSpeed:F2}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Fall Speed: {fallSpeed:F2}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Direction: {velocity:F3}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Grounded: {grounded}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Falling: {falling}", style);
        y += lineHeight;

        GUI.Label(new Rect(x, y, panelWidth - 16, lineHeight), $"Jumping: {jumping}", style);
    }
}