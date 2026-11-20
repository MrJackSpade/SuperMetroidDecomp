namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The staircase outline of Kraid's body that his body and projectile collision test against.</summary>
public static class KraidBodyContour
{
    /// <summary>
    /// The left edge of Kraid's body at <paramref name="relativeY"/>: each ledge of the staircase holds its edge from
    /// its top down to the next ledge, and everything at or below his origin takes the lowest ledge's edge.
    /// </summary>
    /// <param name="relativeY">The vertical distance from Kraid's body to the probe, in pixels; negative is above him.</param>
    public static int LeftEdge(int relativeY) => relativeY switch
    {
        >= 0 => -48,
        >= -32 => -32,
        >= -48 => -24,
        >= -80 => -8,
        >= -112 => 0,
        _ => 8,
    };
}
