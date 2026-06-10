using System;
using UnityEngine;

namespace Treasures.Bots
{
    /// <summary>
    /// A directed edge from one <see cref="ParkourNode"/> to another.
    /// A "jump" link means the bot must leave the ground to reach the target
    /// (gap or height difference between platforms).
    /// </summary>
    [Serializable]
    public class ParkourLink
    {
        [Tooltip("The node this edge leads to.")]
        public ParkourNode target;

        [Tooltip("If true, the bot must jump to traverse this edge (gap between platforms).")]
        public bool isJump;

        [Tooltip("If true, the bot should use a second (air) jump to cover a long/high gap.")]
        public bool requiresDoubleJump;
    }
}
