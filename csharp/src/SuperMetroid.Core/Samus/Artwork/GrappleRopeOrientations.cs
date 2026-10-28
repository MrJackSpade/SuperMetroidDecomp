namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which rope picture each grapple angle shows.</summary>
public static class GrappleRopeOrientations
{
    /// <summary>
    /// The rope picture the grapple's angle selects: `$9B:C005` folds the unsigned angle into one of 64 sectors, and
    /// the sectors repeat every half turn.
    /// </summary>
    public static GrappleRopeOrientation For(RotationAngle angle)
    {
        // The 64 sectors repeat after half a turn. Reflect each half-turn about
        // its midpoint, retaining the native three/eight/five sector widths.
        int halfTurnSector = (angle.RawValue >> 10) & 31;
        int distance = Math.Min(halfTurnSector, 31 - halfTurnSector);
        return distance < 3 ? GrappleRopeOrientation.Vertical : distance < 11 ? GrappleRopeOrientation.Diagonal : GrappleRopeOrientation.Horizontal;
    }
}
