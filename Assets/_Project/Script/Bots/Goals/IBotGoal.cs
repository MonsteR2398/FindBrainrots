using UnityEngine;

namespace Treasures.Bots
{
    /// <summary>
    /// Shared data passed to goals so they can choose a destination node.
    /// </summary>
    public class BotContext
    {
        public ParkourGraph Graph;
        public BotLocomotion Locomotion;
        public Transform Self;
        public ParkourNode CurrentNode;

        public BotContext(ParkourGraph graph, BotLocomotion locomotion, Transform self)
        {
            Graph = graph;
            Locomotion = locomotion;
            Self = self;
        }
    }

    /// <summary>
    /// A behaviour goal that decides which node the bot should head toward next.
    /// Implementations stay decoupled from locomotion and the graph traversal.
    /// </summary>
    public interface IBotGoal
    {
        /// <summary>Pick the next destination node, or null if none available.</summary>
        ParkourNode SelectTarget(BotContext ctx);
    }
}
