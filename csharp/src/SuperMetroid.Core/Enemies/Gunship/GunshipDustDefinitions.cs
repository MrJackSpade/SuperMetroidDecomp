namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's liftoff dust clouds.</summary>
public static class GunshipDustDefinitions
{
    /// <summary>Dust is eighty pixels below Samus.</summary>
    public const int YOffset = 80;

    /// <summary>
    /// The six dust clouds the gunship raises as its engines fire, in the order it spawns them: the inner, middle and
    /// outer clouds right of Samus, then the same three on her left, each with its horizontal offset from Samus and the
    /// typed animation (#641) it starts.
    /// </summary>
    internal static IReadOnlyList<GunshipDustPlacement> LiftoffClouds { get; } =
    [
        new GunshipDustPlacement(64, GunshipDustSegment.RightInner),
        new GunshipDustPlacement(72, GunshipDustSegment.RightMiddle),
        new GunshipDustPlacement(80, GunshipDustSegment.RightOuter),
        new GunshipDustPlacement(-64, GunshipDustSegment.LeftInner),
        new GunshipDustPlacement(-72, GunshipDustSegment.LeftMiddle),
        new GunshipDustPlacement(-80, GunshipDustSegment.LeftOuter),
    ];
}
