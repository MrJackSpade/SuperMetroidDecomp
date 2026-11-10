namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which rope pictures each grapple angle shows.</summary>
public static class GrappleRopeOrientations
{
    /// <summary>The sixty-fourths of a turn the rope's angle is told apart in.</summary>
    private const int Sectors = 64;

    /// <summary>The sectors of a half turn, after which the rope's pictures repeat.</summary>
    private const int HalfTurnSectors = Sectors / 2;

    /// <summary>The sectors either side of upright whose rope shows the vertical pictures.</summary>
    private const int VerticalSectors = 3;

    /// <summary>The sectors beyond the vertical ones whose rope shows the diagonal pictures.</summary>
    private const int DiagonalSectors = 8;

    /// <summary>
    /// The rope pictures the grapple's angle selects, from how many sectors the angle lies from upright in its half turn.
    /// </summary>
    public static GrappleRopeOrientation For(RotationAngle angle)
    {
        int halfTurnSector = angle.Sector(Sectors) % HalfTurnSectors;
        int fromUpright = Math.Min(halfTurnSector, HalfTurnSectors - 1 - halfTurnSector);
        return fromUpright < VerticalSectors ? GrappleRopeOrientation.Vertical
            : fromUpright < VerticalSectors + DiagonalSectors ? GrappleRopeOrientation.Diagonal
            : GrappleRopeOrientation.Horizontal;
    }
}
