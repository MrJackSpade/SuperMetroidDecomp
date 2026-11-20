using static SuperMetroid.Core.Enemies.Choot.ChootFallingPathDefinitions;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Where a falling Choot is: its pattern, and the sample of its path at each update of its fall.</summary>
internal static class ChootFalls
{
    /// <summary>How far, in pixels, the loop origin of <paramref name="fallingPath"/> advances per loop.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    internal static int LoopYDistance(ChootFallingPath fallingPath) =>
        ChootPatternDefinitions.LoopYDistances.TryGetValue(fallingPath, out int distance)
            ? distance
            : throw new InvalidOperationException($"Undefined {nameof(ChootFallingPath)} {fallingPath}.");

    /// <summary>The updates one loop of <paramref name="fallingPath"/> lasts: its samples' updates.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    internal static int LoopUpdates(ChootFallingPath fallingPath) =>
        Paths.TryGetValue(fallingPath, out ChootFallSample[]? path)
            ? path.Sum(sample => sample.Updates)
            : throw new InvalidOperationException($"Undefined {nameof(ChootFallingPath)} {fallingPath}.");

    /// <summary>
    /// The offset from the falling origin of one falling path at <paramref name="update"/> updates into a loop; a loop
    /// lasts no longer than its path's samples.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    /// <exception cref="InvalidDataException"><paramref name="update"/> is negative or lies at or past the path's end.</exception>
    internal static Offset16<RoomPixels> At(ChootFallingPath fallingPath, int update)
    {
        if (!Paths.TryGetValue(fallingPath, out ChootFallSample[]? path))
            throw new InvalidOperationException($"Undefined {nameof(ChootFallingPath)} {fallingPath}.");
        if (update < 0)
            throw new InvalidDataException($"Choot falling-pattern update {update} lies before falling path {fallingPath}.");

        int remaining = update;
        foreach (ChootFallSample sample in path)
        {
            if (remaining < sample.Updates)
                return sample.Offset;
            remaining -= sample.Updates;
        }
        throw new InvalidDataException($"Choot falling-pattern update {update} lies at or past the end of falling path {fallingPath}.");
    }
}
