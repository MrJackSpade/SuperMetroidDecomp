namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>How far each loop of a Choot's falling path carries it down.</summary>
internal static class ChootFalls
{
    /// <summary>How far, in pixels, the loop origin of <paramref name="fallingPath"/> advances per loop.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    internal static int LoopYDistance(ChootFallingPath fallingPath) => Path(fallingPath).LoopYDistance;

    /// <summary>The definition of <paramref name="fallingPath"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fallingPath"/> is not a defined path.</exception>
    private static ChootFallingPathDefinition Path(ChootFallingPath fallingPath) =>
        ChootFallingPathDefinitions.Paths.TryGetValue(fallingPath, out ChootFallingPathDefinition? path)
            ? path
            : throw new InvalidOperationException($"Undefined {nameof(ChootFallingPath)} {fallingPath}.");
}
