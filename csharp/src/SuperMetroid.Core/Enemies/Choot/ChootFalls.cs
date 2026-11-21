using static SuperMetroid.Core.Enemies.Choot.ChootFallingPathDefinitions;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Where a falling Choot is: its pattern, and the offset of its path on each update of a loop of its fall.</summary>
internal static class ChootFalls
{
    /// <summary>
    /// The offset from the falling origin each falling path shows on each update of one loop, laid out once from its
    /// samples, each held for the updates it lists.
    /// </summary>
    private static readonly IdentityTable<ChootFallingPath, Offset16<RoomPixels>[]> LoopOffsets = new(path =>
        [.. Paths[path].Samples.SelectMany(sample => Enumerable.Repeat(sample.Offset, sample.Updates))]);

    /// <summary>How far, in pixels, the loop origin of <paramref name="fallingPath"/> advances per loop.</summary>
    internal static int LoopYDistance(ChootFallingPath fallingPath) => Paths[fallingPath].LoopYDistance;

    /// <summary>
    /// The offset from the falling origin <paramref name="fallingPath"/> shows on each update of one loop; the update after
    /// the last ends the loop.
    /// </summary>
    internal static ReadOnlyMemory<Offset16<RoomPixels>> LoopOf(ChootFallingPath fallingPath) => LoopOffsets[fallingPath];
}
