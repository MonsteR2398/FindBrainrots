using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Example mini-mode: find brainrots before time runs out.
    /// This is a ready-to-extend stub - hook up scoring/UI when you build the mode out.
    /// </summary>
    [CreateAssetMenu(menuName = "World System/Rules/Timed Hunt Rules", fileName = "TimedHuntRules")]
    public class TimedHuntRules : GameRules
    {
        [Tooltip("How long the hunt lasts, in seconds.")]
        public float duration = 60f;

        [System.NonSerialized] public float TimeRemaining;
        [System.NonSerialized] public bool IsRunning;

        /// <summary>Fired when the timer reaches zero.</summary>
        public event System.Action TimeExpired;

        public override void OnEnter(GameModeContext ctx)
        {
            TimeRemaining = duration;
            IsRunning = true;
            Debug.Log($"[Rules] Timed hunt started: {duration:0}s");
        }

        public override void OnExit(GameModeContext ctx)
        {
            IsRunning = false;
        }

        public override void Tick(GameModeContext ctx, float deltaTime)
        {
            if (!IsRunning) return;

            TimeRemaining -= deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                IsRunning = false;
                Debug.Log("[Rules] Time's up!");
                TimeExpired?.Invoke();
            }
        }
    }
}
