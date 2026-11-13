namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the spike dispatchers do with a spike's reaction (#1722).</summary>
public static class SpikeReactions
{
    /// <summary>
    /// Whether <paramref name="reaction"/> is one the room's area gives spikes, which Samus's swing and the grapple beam
    /// pass by.
    /// </summary>
    public static bool IsArea(SpikeReaction reaction) =>
        reaction is SpikeReaction.AreaHarmless or SpikeReaction.AreaEnemyBreakable or SpikeReaction.AreaPastEnemyTable;
}
