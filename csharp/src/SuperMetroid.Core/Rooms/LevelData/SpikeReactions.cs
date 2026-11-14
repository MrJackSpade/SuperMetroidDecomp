namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the spike dispatchers do with a spike's reaction (#1722).</summary>
public static class SpikeReactions
{
    /// <summary>What the grapple beam makes of a spike reaction.</summary>
    extension(SpikeReaction reaction)
    {
        /// <summary>Whether the grapple beam passes through a spike block with the reaction rather than stopping on it.</summary>
        public bool LetsGrappleBeamThrough =>
            reaction is SpikeReaction.HarmlessLetsGrappleThrough or SpikeReaction.EnemyBreakableLetsGrappleThrough;
    }
}
