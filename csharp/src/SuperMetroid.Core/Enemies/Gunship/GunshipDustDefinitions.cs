namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's liftoff dust clouds.</summary>
public static class GunshipDustDefinitions
{
    /// <summary>Dust is eighty pixels below Samus.</summary>
    public const int YOffset = 80;

    /// <summary>
    /// The six dust clouds the gunship raises as its engines fire, in the order it spawns them: the inner, middle and
    /// outer clouds right of Samus, then the same three on her left, each with its horizontal offset from Samus and the
    /// typed program (#641) it starts.
    /// </summary>
    internal static IReadOnlyList<GunshipDustPlacement> LiftoffClouds { get; } =
    [
        new GunshipDustPlacement(64, GunshipDustProgram.RightInner),
        new GunshipDustPlacement(72, GunshipDustProgram.RightMiddle),
        new GunshipDustPlacement(80, GunshipDustProgram.RightOuter),
        new GunshipDustPlacement(-64, GunshipDustProgram.LeftInner),
        new GunshipDustPlacement(-72, GunshipDustProgram.LeftMiddle),
        new GunshipDustPlacement(-80, GunshipDustProgram.LeftOuter),
    ];
}
