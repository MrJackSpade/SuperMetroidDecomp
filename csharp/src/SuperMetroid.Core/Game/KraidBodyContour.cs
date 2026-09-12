namespace SuperMetroid.Core.Game;

/// <summary>Compiled staircase used by Kraid's private body/projectile collision path.</summary>
public static class KraidBodyContour
{
    /// <summary>
    /// $A7:B163/B165, HitboxDefinitionTable_KraidBody: each left edge is paired
    /// with the next record's top boundary. The final signed minimum terminates
    /// the contour for every signed relative Y coordinate.
    /// </summary>
    public static short LeftEdge(short relativeY) => relativeY switch
    {
        >= 16 => -48,
        >= 0 => -48,
        >= -32 => -32,
        >= -48 => -24,
        >= -80 => -8,
        >= -112 => 0,
        _ => 8,
    };
}
