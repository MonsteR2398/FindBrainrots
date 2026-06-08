using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Default "just explore" rules. The world is the same, only the location changes.
    /// No win/lose conditions, no timer.
    /// </summary>
    [CreateAssetMenu(menuName = "World System/Rules/Explore Rules", fileName = "ExploreRules")]
    public class ExploreRules : GameRules
    {
        public override void OnEnter(GameModeContext ctx)
        {
            Debug.Log($"[Rules] Explore mode entered: {(ctx.Mode != null ? ctx.Mode.DisplayName : "?")}");
        }
    }
}
