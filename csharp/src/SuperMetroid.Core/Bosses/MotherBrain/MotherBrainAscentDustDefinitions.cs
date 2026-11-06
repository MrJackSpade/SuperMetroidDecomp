namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where the floor dust rises while Mother Brain stands up for her second phase.</summary>
internal static class MotherBrainAscentDustDefinitions
{
    /// <summary>The room positions the ascent's dust clouds cycle through on the floor, selected by the frames left in the rise.</summary>
    internal static readonly Position16<RoomPixels>[] DustPositions =
    [
        new(61, 212),
        new(84, 212),
        new(32, 212),
        new(53, 212),
        new(90, 212),
        new(67, 212),
        new(103, 212),
        new(41, 212),
    ];
}
