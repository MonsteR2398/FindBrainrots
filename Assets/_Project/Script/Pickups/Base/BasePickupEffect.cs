using UnityEngine;

namespace Treasures.Pickups
{
    public abstract class BasePickupEffect : ScriptableObject
    {
        /// <summary>
        /// Executes the effect when an object is picked up.
        /// </summary>
        /// <param name="picker">The GameObject that triggered the pickup (usually the player).</param>
        public abstract void Apply(GameObject picker);
    }
}
