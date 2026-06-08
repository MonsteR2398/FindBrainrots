using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Runtime context passed to a mode's rules while it is active.
    /// </summary>
    public class GameModeContext
    {
        public PlayerController Player;
        public Scene WorldScene;
        public GameModeDefinition Mode;

        public GameModeContext(PlayerController player, Scene worldScene, GameModeDefinition mode)
        {
            Player = player;
            WorldScene = worldScene;
            Mode = mode;
        }
    }

    /// <summary>
    /// Strategy object describing how a game mode behaves. Subclass to add new modes
    /// (e.g. timed hunts). The active mode's rules receive lifecycle callbacks from
    /// the <see cref="GameModeManager"/>.
    /// </summary>
    public abstract class GameRules : ScriptableObject
    {
        /// <summary>Called once after the world scene is loaded and the player has been placed.</summary>
        public virtual void OnEnter(GameModeContext ctx) { }

        /// <summary>Called once before the current world is unloaded.</summary>
        public virtual void OnExit(GameModeContext ctx) { }

        /// <summary>Called every frame while this mode is active (not while switching).</summary>
        public virtual void Tick(GameModeContext ctx, float deltaTime) { }
    }
}
