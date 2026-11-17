namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where the floor dust rises while Mother Brain stands up for her second phase.</summary>
internal static class MotherBrainAscentDustDefinitions
{
    /// <summary>
    /// The room positions on the floor the ascent's dust clouds burst at, in the order they burst (#2315), continuing
    /// the place her burst cycle stands at.
    /// </summary>
    internal static IReadOnlyList<Position16<RoomPixels>> DustPositions { get; } =
    [
        new(41, 212),
        new(103, 212),
        new(67, 212),
        new(90, 212),
        new(53, 212),
        new(32, 212),
        new(84, 212),
        new(61, 212),
    ];
}
