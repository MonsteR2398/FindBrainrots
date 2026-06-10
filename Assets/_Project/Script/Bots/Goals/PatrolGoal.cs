namespace Treasures.Bots
{
    /// <summary>
    /// Default ambient behaviour: wander to a random node in the graph,
    /// then pick another. Makes bots roam and parkour around the level.
    /// </summary>
    public class PatrolGoal : IBotGoal
    {
        public ParkourNode SelectTarget(BotContext ctx)
        {
            if (ctx == null || ctx.Graph == null) return null;
            return ctx.Graph.RandomNode(ctx.CurrentNode);
        }
    }
}
