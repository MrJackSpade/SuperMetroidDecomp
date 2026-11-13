namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which rope pictures each grapple angle shows.</summary>
public static class GrappleRopeOrientations
{
    /// <summary>
    /// How far either side of upright the rope shows the vertical pictures: from twelve whole steps before it up to, but
    /// not including, twelve after it.
    /// </summary>
    private static readonly TrigonometryAngle VerticalReach = new(12);

    /// <summary>How far either side of upright, beyond the vertical reach, the rope shows the diagonal pictures.</summary>
    private static readonly TrigonometryAngle DiagonalReach = new(44);

    /// <summary>
    /// The rope pictures the grapple's angle selects, from how far its whole step lies from upright: the pictures repeat
    /// every half turn, so the angle is carried a quarter turn on into the first half turn, where upright lies at its
    /// middle and the horizontal at its two ends.
    /// </summary>
    public static GrappleRopeOrientation For(RotationAngle angle)
    {
        TrigonometryAngle across = (angle.ToTrigonometryAngle() + TrigonometryAngle.QuarterTurn).InFirstHalfTurn();
        return Within(across, VerticalReach) ? GrappleRopeOrientation.Vertical
            : Within(across, DiagonalReach) ? GrappleRopeOrientation.Diagonal
            : GrappleRopeOrientation.Horizontal;
    }

    /// <summary>
    /// Whether <paramref name="across"/>, carried into the first half turn with upright at its middle, lies within
    /// <paramref name="reach"/> before upright or short of <paramref name="reach"/> after it.
    /// </summary>
    private static bool Within(TrigonometryAngle across, TrigonometryAngle reach) =>
        across >= TrigonometryAngle.QuarterTurn - reach && across < TrigonometryAngle.QuarterTurn + reach;
}
