namespace SuperMetroid.Core.Game;

/// <summary>Compiled staircase used by Kraid's private body/projectile collision path.</summary>
public static class KraidBodyContour
{
    /// <summary>$A7:B161, first bottom boundary of HitboxDefinitionTable_KraidBody.</summary>
    private const short FirstBottomBoundary = 0x03ff;

    /// <summary>
    /// $A7:B163/B165, HitboxDefinitionTable_KraidBody: each left edge is paired
    /// with the next record's top boundary. The final signed minimum terminates
    /// the contour for every signed relative Y coordinate.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the complete native table and both
    /// CMP/BPL comparisons at $A7:B24B/B250. The first bottom comparison wraps to
    /// nonnegative for Y=-32768..-31746, selecting -48 before the ordinary staircase.
    /// Remaining inputs select the first inclusive top boundary. Preserve this
    /// wrapped-sign test, rather than replacing it with ordered signed comparison.
    /// </remarks>
    public static short LeftEdge(short relativeY) => relativeY switch
    {
        <= short.MinValue + FirstBottomBoundary - 1 => -48,
        >= 0 => -48,
        >= -32 => -32,
        >= -48 => -24,
        >= -80 => -8,
        >= -112 => 0,
        _ => 8,
    };
}
