namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's getaway as its zoom and drift curves play it out (#2247): the zoom and the drift each update shows, for
/// the getaway's duration. The getaway ends on the update after its last, once both curves are exhausted. The curves are
/// laid out once into one step per update (#2553), so the flight walks them and no update outside them exists.
/// </summary>
public static partial class CeresRidleyGetawayCurve
{
    /// <summary>Each update of the getaway in turn: the floor's zoom and drift on it.</summary>
    /// <exception cref="InvalidDataException">The zoom and drift curves last different numbers of updates.</exception>
    public static IReadOnlyList<CeresRidleyGetawayStep> Steps { get; } = LaidOut();

    /// <summary>
    /// The curves laid out one step per update: each update of the zoom curve, with the drift its own curve has reached by
    /// then. The two curves advance side by side, each through its own stretches, and last as long as each other.
    /// </summary>
    private static CeresRidleyGetawayStep[] LaidOut()
    {
        var steps = new List<CeresRidleyGetawayStep>();
        int driftStretch = 0;
        UpdateCountdown driftLeft = UpdateCountdown.For(CeresRidleyGetawayDefinitions.DriftCurve[driftStretch].Updates);
        foreach (CeresRidleyGetawayZoomStretch zoom in CeresRidleyGetawayDefinitions.ZoomCurve)
        {
            foreach (int update in Enumerable.Range(0, zoom.Updates))
            {
                steps.Add(new CeresRidleyGetawayStep(zoom.ZoomAfter(update), CeresRidleyGetawayDefinitions.DriftCurve[driftStretch].Drift));
                // The drift curve moves on to its next stretch once this one's updates are shown, as the last one's are
                // on the getaway's last update.
                if (driftLeft.StepEnds() && ++driftStretch < CeresRidleyGetawayDefinitions.DriftCurve.Count)
                    driftLeft.Restart(CeresRidleyGetawayDefinitions.DriftCurve[driftStretch].Updates);
            }
        }
        return [.. steps];
    }
}
