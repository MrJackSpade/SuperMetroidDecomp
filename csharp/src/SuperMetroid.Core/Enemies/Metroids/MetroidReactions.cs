namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How an ordinary Metroid flees when bombed off Samus, and which cry it plays.</summary>
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

    /// <summary>The cry <paramref name="random"/>'s <see cref="MetroidBehaviorDefinitions.CrySlot"/> selects.</summary>
    internal static SoundEffectId RandomCrySoundEffect(int random) =>
        MetroidBehaviorDefinitions.RandomCries[MetroidBehaviorDefinitions.CrySlot.Of(random)];
}
