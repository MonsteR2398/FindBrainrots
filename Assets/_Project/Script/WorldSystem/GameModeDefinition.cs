using Treasures.CurrencySystem;
using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Data describing one selectable world/mode: which scene to load, how it appears
    /// on the portal button, and which rules govern it.
    /// </summary>
    [CreateAssetMenu(menuName = "World System/Game Mode Definition", fileName = "GameMode_")]
    public class GameModeDefinition : ScriptableObject
    {
        [Tooltip("Stable identifier, useful for saves/analytics.")]
        public string Id;

        [Tooltip("Shown on the portal button.")]
        public string DisplayName = "New World";

        [TextArea] public string Description;

        [Tooltip("Optional icon for the portal button.")]
        public Sprite Icon;

        [Tooltip("Accent color for the portal button.")]
        public Color AccentColor = new Color(0.2f, 0.5f, 1f, 1f);

        [Tooltip("Name of the world Scene to load additively. Must be added to Build Settings.")]
        public string SceneName;

        [Tooltip("Rules applied while this mode is active. Leave empty for plain free-roam.")]
        public GameRules Rules;

        public CurrencyValue price;
    }
}
