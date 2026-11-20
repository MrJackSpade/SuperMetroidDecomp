namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's getaway as its zoom and drift curves play it out (#2247): the zoom and the drift each update shows, for
/// the getaway's duration. The getaway ends on the update after its last, once both curves are exhausted. The curves are
/// laid out once into one step per update (#2553), so the flight walks them and no update outside them exists.
/// </summary>
public static class CeresRidleyGetawayCurve
{
    /// <summary>Each update of the getaway in turn: the floor's zoom and drift on it.</summary>
    /// <exception cref="InvalidDataException">The zoom and drift curves last different numbers of updates.</exception>
    public static IReadOnlyList<CeresRidleyGetawayStep> Steps { get; } = LaidOut();

    /// <summary>The getaway's duration in updates: every update of its zoom curve, which its drift curve matches.</summary>
    public static int Duration => Steps.Count;

    /// <summary>The curves laid out one step per update, which must last as long as each other.</summary>
    private static CeresRidleyGetawayStep[] LaidOut()
    {
        FloorZoom[] zooms = [.. CeresRidleyGetawayDefinitions.ZoomCurve
            .SelectMany(stretch => Enumerable.Range(0, stretch.Updates).Select(stretch.ZoomAfter))];
        Offset16<FloorPixels>[] drifts = [.. CeresRidleyGetawayDefinitions.DriftCurve
            .SelectMany(stretch => Enumerable.Repeat(stretch.Drift, stretch.Updates))];
        return zooms.Length == drifts.Length
            ? [.. zooms.Zip(drifts, (zoom, drift) => new CeresRidleyGetawayStep(zoom, drift))]
            : throw new InvalidDataException($"Ceres Ridley's getaway zoom lasts {zooms.Length} updates but its drift {drifts.Length}.");
    }
}

/// <summary>One update of Ceres Ridley's getaway (#2247, #2553).</summary>
/// <param name="Zoom">The floor's zoom on the update.</param>
/// <param name="Drift">
/// The floor's drift on the update: its X is subtracted from the floor's horizontal offset, so the image moves right, and
/// its Y added to the vertical offset.
/// </param>
public readonly record struct CeresRidleyGetawayStep(FloorZoom Zoom, Offset16<FloorPixels> Drift);
