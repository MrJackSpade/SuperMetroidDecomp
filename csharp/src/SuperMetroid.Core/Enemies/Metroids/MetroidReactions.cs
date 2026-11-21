namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The ordinary Metroid's own reactions: those its definition names for Samus's touch and for her shots and bombs
/// (#2558), which its entity answers, and how it flees when bombed off Samus.
/// </summary>
internal static class MetroidReactions
{
    /// <summary>The Metroid's own reaction to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Metroid.Touch");

    /// <summary>The Metroid's own reaction to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Metroid.Shot");

    /// <summary>
    /// The move an escaping Metroid with <paramref name="movesLeft"/> of its escape's moves left makes: the escape's moves
    /// in order (<see cref="MetroidBehaviorDefinitions.EscapeMoves"/>), the first with all of them left.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No move, or more moves than the escape makes, are left.</exception>
    internal static Offset16<RoomPixels> EscapeDisplacement(int movesLeft) =>
        MetroidBehaviorDefinitions.EscapeMoves[^Ensure.BetweenInclusive(1, MetroidBehaviorDefinitions.EscapeMoves.Length, movesLeft)];
}
