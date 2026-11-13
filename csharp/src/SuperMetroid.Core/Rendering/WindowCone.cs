namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A cone of light from an apex (#1468): every pixel between two edge rays leaving the apex, sweeping clockwise from
/// <see cref="FirstEdge"/> to <see cref="SecondEdge"/>. Edges are angles clockwise from straight up the screen. Two
/// equal edges make a ray one pixel wide, never an infinite line. The apex may lie off the screen, and the cone is
/// narrower than half a turn. The X-ray scope, the morph-ball eye's beam and Mother Brain's rainbow beam are cones.
/// </summary>
public sealed record WindowCone : WindowShape
{
    /// <summary>The pixel the cone's edges leave from.</summary>
    public Offset16<ScreenPixels> Apex { get; }

    /// <summary>The edge the cone sweeps clockwise from.</summary>
    public TrigonometryAngle FirstEdge { get; }

    /// <summary>The edge the cone sweeps clockwise to.</summary>
    public TrigonometryAngle SecondEdge { get; }

    /// <summary>Creates a cone.</summary>
    /// <param name="apex">The pixel the cone's edges leave from.</param>
    /// <param name="firstEdge">The edge the cone sweeps clockwise from.</param>
    /// <param name="secondEdge">The edge the cone sweeps clockwise to.</param>
    /// <exception cref="ArgumentException">The clockwise sweep from the first edge to the second is half a turn or more.</exception>
    public WindowCone(Offset16<ScreenPixels> apex, TrigonometryAngle firstEdge, TrigonometryAngle secondEdge)
    {
        Apex = apex;
        FirstEdge = firstEdge;
        SecondEdge = secondEdge;
        if (SecondEdge - FirstEdge >= TrigonometryAngle.HalfTurn)
            throw new ArgumentException("A window cone must be narrower than half a turn.", nameof(secondEdge));
    }
}
