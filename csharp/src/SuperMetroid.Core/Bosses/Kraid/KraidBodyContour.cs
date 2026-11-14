namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled staircase used by Kraid's private body/projectile collision path.</summary>
public static class KraidBodyContour
{
    /// <summary>
    /// The left edge of Kraid's body at <paramref name="relativeY"/>: each ledge of the staircase holds its edge from
    /// its top down to the next ledge, and everything at or below his origin takes the lowest ledge's edge.
    /// </summary>
    /// <remarks>
    /// In the original, a probe more than 31,745 pixels above his origin took the lowest ledge's edge too (#1876). No room
    /// is that tall (at most <see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/> screens), so the staircase is the
    /// same at every height a projectile or Samus can reach.
    /// </remarks>
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
