namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>How far each loop of a Choot's falling path carries it down.</summary>
internal static class ChootFalls
{
    /// <summary>How far, in pixels, the loop origin of <paramref name="fallingPath"/> advances per loop.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    internal static int LoopYDistance(ChootFallingPath fallingPath) =>
        ChootPatternDefinitions.LoopYDistances.TryGetValue(fallingPath, out int distance)
            ? distance
            : throw new InvalidOperationException($"Undefined {nameof(ChootFallingPath)} {fallingPath}.");
}
