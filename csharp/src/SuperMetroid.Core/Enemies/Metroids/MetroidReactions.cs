namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How an ordinary Metroid flees when bombed off Samus.</summary>
internal static class MetroidReactions
{
    /// <summary>
    /// The move an escaping Metroid with <paramref name="movesLeft"/> of its escape's moves left makes: the escape's moves
    /// in order (<see cref="MetroidBehaviorDefinitions.EscapeMoves"/>), the first with all of them left.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No move, or more moves than the escape makes, are left.</exception>
    internal static Offset16<RoomPixels> EscapeDisplacement(int movesLeft) =>
        MetroidBehaviorDefinitions.EscapeMoves[MetroidBehaviorDefinitions.EscapeDuration -
            Ensure.BetweenInclusive(1, MetroidBehaviorDefinitions.EscapeDuration, movesLeft)];
}
