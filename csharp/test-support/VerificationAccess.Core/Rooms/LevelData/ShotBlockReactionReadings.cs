namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A check's reading of which shot reaction handles a block's shootable reaction (#2554): the reaction, or none for a
/// shootable reaction no shot handles, which the checks enumerate over every block behaviour.
/// </summary>
internal static class ShotBlockReactionReadings
{
    /// <summary>The shot reaction handling <paramref name="reaction"/>, or null when no shot handles it.</summary>
    internal static ShotBlockReaction? HandledOrNone(ShootableReaction reaction) =>
        ShotBlockReactions.TryOf(reaction, out ShotBlockReaction handled) ? handled : null;
}
