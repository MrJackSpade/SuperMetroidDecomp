namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>How far each loop of a Choot's falling path carries it down.</summary>
internal static class ChootFalls
{
    /// <summary>How far, in pixels, the loop origin of <paramref name="fallingPath"/> advances per loop.</summary>
    internal static int LoopYDistance(ChootFallingPath fallingPath) => Path(fallingPath).LoopYDistance;

    /// <summary>The definition of <paramref name="fallingPath"/>.</summary>
    private static ChootFallingPathDefinition Path(ChootFallingPath fallingPath) => ChootFallingPathDefinitions.Paths[fallingPath];
}
