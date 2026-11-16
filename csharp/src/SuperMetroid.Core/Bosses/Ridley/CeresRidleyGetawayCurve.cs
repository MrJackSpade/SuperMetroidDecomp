namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's getaway as its zoom and drift curves play it out (#2247): the zoom and the drift each update shows, for
/// the getaway's duration. The getaway ends on the update after its last, once both curves are exhausted.
/// </summary>
public static class CeresRidleyGetawayCurve
{
    /// <summary>The getaway's duration in updates: every update of its zoom curve, which its drift curve matches.</summary>
    /// <exception cref="InvalidDataException">The zoom and drift curves last different numbers of updates.</exception>
    public static int Duration { get; } = MatchingDuration();

    /// <summary>The floor's zoom on update <paramref name="update"/> of the getaway, from zero.</summary>
    /// <exception cref="InvalidDataException">The update lies outside the getaway.</exception>
    public static FloorZoom ZoomAt(int update)
    {
        int updatesIn = RequireWithin(update);
        foreach (CeresRidleyGetawayZoomStretch stretch in CeresRidleyGetawayDefinitions.ZoomCurve)
        {
            if (updatesIn < stretch.Updates)
                return stretch.ZoomAfter(updatesIn);
            updatesIn -= stretch.Updates;
        }
        throw new InvalidDataException($"Ceres Ridley getaway update {update} lies past its zoom curve.");
    }

    /// <summary>
    /// The floor's drift on update <paramref name="update"/> of the getaway, from zero: its X is subtracted from the
    /// floor's horizontal offset, so the image moves right, and its Y added to the vertical offset.
    /// </summary>
    /// <exception cref="InvalidDataException">The update lies outside the getaway.</exception>
    public static Offset16<FloorPixels> DriftAt(int update)
    {
        int updatesIn = RequireWithin(update);
        foreach (CeresRidleyGetawayDriftStretch stretch in CeresRidleyGetawayDefinitions.DriftCurve)
        {
            if (updatesIn < stretch.Updates)
                return stretch.Drift;
            updatesIn -= stretch.Updates;
        }
        throw new InvalidDataException($"Ceres Ridley getaway update {update} lies past its drift curve.");
    }

    /// <summary>Returns <paramref name="update"/>, rejecting one outside the getaway's duration.</summary>
    private static int RequireWithin(int update) =>
        update >= 0 && update < Duration
            ? update
            : throw new InvalidDataException($"Ceres Ridley getaway update {update} lies outside its {Duration} updates.");

    /// <summary>The updates both curves last, which must agree.</summary>
    private static int MatchingDuration()
    {
        int zoom = CeresRidleyGetawayDefinitions.ZoomCurve.Sum(stretch => stretch.Updates);
        int drift = CeresRidleyGetawayDefinitions.DriftCurve.Sum(stretch => stretch.Updates);
        return zoom == drift
            ? zoom
            : throw new InvalidDataException($"Ceres Ridley's getaway zoom lasts {zoom} updates but its drift {drift}.");
    }
}
