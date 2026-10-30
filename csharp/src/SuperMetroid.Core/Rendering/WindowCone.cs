namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A cone of light from an apex (#1468): every pixel between two edge rays leaving the apex, sweeping clockwise from
/// <see cref="FirstEdge"/> to <see cref="SecondEdge"/>. Edges are angles in <see cref="AngleUnitsPerTurn"/>ths of a turn,
/// clockwise from straight up the screen. Two equal edges make a ray one pixel wide, never an infinite line. The apex
/// may lie off the screen, and the cone is narrower than half a turn. The X-ray scope, the morph-ball eye's beam and
/// Mother Brain's rainbow beam are cones.
/// </summary>
public sealed record WindowCone : WindowShape
{
    /// <summary>The angle units in one complete turn.</summary>
    public const int AngleUnitsPerTurn = RotationAngle.TableUnitsPerTurn;

    /// <summary>The pixel the cone's edges leave from.</summary>
    public Offset16<ScreenPixels> Apex { get; }

    /// <summary>The edge the cone sweeps clockwise from, 0 through <see cref="AngleUnitsPerTurn"/> less one.</summary>
    public int FirstEdge { get; }

    /// <summary>The edge the cone sweeps clockwise to, 0 through <see cref="AngleUnitsPerTurn"/> less one.</summary>
    public int SecondEdge { get; }

    /// <summary>Creates a cone, wrapping each edge angle into one turn.</summary>
    /// <param name="apex">The pixel the cone's edges leave from.</param>
    /// <param name="firstEdge">The edge the cone sweeps clockwise from, in any whole number of turns.</param>
    /// <param name="secondEdge">The edge the cone sweeps clockwise to, in any whole number of turns.</param>
    /// <exception cref="ArgumentException">The clockwise sweep from the first edge to the second is half a turn or more.</exception>
    public WindowCone(Offset16<ScreenPixels> apex, int firstEdge, int secondEdge)
    {
        Apex = apex;
        FirstEdge = WithinTurn(firstEdge);
        SecondEdge = WithinTurn(secondEdge);
        if (WithinTurn(SecondEdge - FirstEdge) >= AngleUnitsPerTurn / 2)
            throw new ArgumentException("A window cone must be narrower than half a turn.", nameof(secondEdge));
    }

    /// <summary>The same angle within one turn.</summary>
    private static int WithinTurn(int angle) => (angle % AngleUnitsPerTurn + AngleUnitsPerTurn) % AngleUnitsPerTurn;
}
