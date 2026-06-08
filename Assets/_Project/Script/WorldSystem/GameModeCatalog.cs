using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// The list of worlds/modes available from the portal. Add new worlds by creating
    /// a GameModeDefinition and dropping it in here - no code changes needed.
    /// </summary>
    [CreateAssetMenu(menuName = "World System/Game Mode Catalog", fileName = "GameModeCatalog")]
    public class GameModeCatalog : ScriptableObject
    {
        public GameModeDefinition[] Modes;
    }
}
