namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How an ordinary Metroid flees when bombed off Samus, and which cry it plays.</summary>
internal static class MetroidReactions
{
    /// <summary>
    /// The move an escaping Metroid with <paramref name="movesLeft"/> of its escape's moves left makes: the escape's moves
    /// in order (<see cref="MetroidBehaviorDefinitions.EscapeMoves"/>), the first with all of them left. A Metroid frozen
    /// while it escapes counts its moves down as it strains against the ice, and one that thaws with none left goes on
    /// round the moves in order.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">More moves are left than the escape makes.</exception>
    internal static Offset16<RoomPixels> EscapeDisplacement(int movesLeft) =>
        MetroidBehaviorDefinitions.EscapeMoves[(MetroidBehaviorDefinitions.EscapeDuration - movesLeft) % MetroidBehaviorDefinitions.EscapeMoves.Length];

    /// <summary>The cry <paramref name="random"/>'s <see cref="MetroidBehaviorDefinitions.CrySlot"/> selects.</summary>
    internal static SoundEffectId RandomCrySoundEffect(int random) =>
        MetroidBehaviorDefinitions.RandomCries[MetroidBehaviorDefinitions.CrySlot.Of(random)];
}
