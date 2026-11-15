namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A cone of light from an apex (#1468): every pixel between two edge rays leaving the apex, sweeping clockwise from
/// <see cref="FirstEdge"/> to <see cref="SecondEdge"/>. Edges are angles clockwise from straight up the screen. Two
/// equal edges make a ray one pixel wide, never an infinite line. The apex may lie off the screen, and the cone is
/// narrower than half a turn. The X-ray scope, the morph-ball eye's beam and Mother Brain's rainbow beam are cones.
/// </summary>
/// <remarks>
/// A pixel is inside when, offset from the apex, it lies no more than <see cref="EdgeAllowance"/> outside either edge
/// and not behind the apex, each measured exactly in whole numbers against the edges' <see cref="ConeEdgeDirection"/>s,
/// so every device shows the same pixels (#2009): the cross product of <see cref="FirstEdgeDirection"/> and the offset
/// is at least minus the allowance, so is the cross product of the offset and <see cref="SecondEdgeDirection"/>, and the
/// offset's dot product with the two directions together is not negative.
/// </remarks>
public sealed record WindowCone : WindowShape
{
    /// <summary>The length of a step along an edge in its <see cref="ConeEdgeDirection"/>: the peak of the game's fine sine.</summary>
    public const int EdgeLength = 32767;

    /// <summary>
    /// How far outside an edge a pixel may lie and stay inside the cone, as the cross product of the edge's direction
    /// and the pixel's offset from the apex: half a pixel, so two equal edges make a ray one pixel wide.
    /// </summary>
    public const int EdgeAllowance = (EdgeLength + 1) / 2;

    /// <summary>The pixel the cone's edges leave from.</summary>
    public Offset16<ScreenPixels> Apex { get; }

    /// <summary>The edge the cone sweeps clockwise from.</summary>
    public TrigonometryAngle FirstEdge { get; }

    /// <summary>The edge the cone sweeps clockwise to.</summary>
    public TrigonometryAngle SecondEdge { get; }

    /// <summary>The way <see cref="FirstEdge"/> leaves the apex.</summary>
    public ConeEdgeDirection FirstEdgeDirection => EdgeDirection(FirstEdge);

    /// <summary>The way <see cref="SecondEdge"/> leaves the apex.</summary>
    public ConeEdgeDirection SecondEdgeDirection => EdgeDirection(SecondEdge);

    /// <summary>
    /// The way an edge at <paramref name="edge"/>, clockwise from straight up, leaves the apex: the game's fine sine of
    /// the angle across and its fine cosine up, the game's own fixed-point steps rather than any device's sine.
    /// </summary>
    public static ConeEdgeDirection EdgeDirection(TrigonometryAngle edge) =>
        new(Trigonometry.FineSignedSine(edge), -Trigonometry.FineSignedSine(edge + TrigonometryAngle.QuarterTurn));

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
