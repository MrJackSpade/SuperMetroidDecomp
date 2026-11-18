namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's getaway: its zoom and drift curves, how it turns the floor and when it shoves Samus (#2247).</summary>
public static class CeresRidleyGetawayDefinitions
{
    /// <summary>The floor's zoom as the getaway sets it up, before its first update: eight floor pixels per screen pixel.</summary>
    internal static FloorZoom InitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(8);

    /// <summary>The floor's scroll as the getaway sets it up, before its first update.</summary>
    internal static Offset16<FloorPixels> InitialFloorOffset { get; } = new(-128, 32);

    /// <summary>The point of the floor the getaway turns and zooms it about as it sets it up.</summary>
    internal static Offset16<FloorPixels> InitialFloorCentre { get; } = new(64, 64);

    /// <summary>The getaway floor's angle as it is set up: a 256th of a step past no turn.</summary>
    internal static RotationAngle InitialAngle { get; } = RotationAngle.FromSteps(0, 1);

    /// <summary>How far the getaway turns its floor each update: 48 256ths of a step.</summary>
    internal static RotationAngle TurnPerFrame { get; } = RotationAngle.FromSteps(0, 48);

    /// <summary>
    /// The update of the getaway on which Ridley's shove takes Samus's movement, so the turning boss image cannot overlap
    /// her from the following update on.
    /// </summary>
    internal const int SamusShoveUpdate = 104;

    /// <summary>The zoom curve's slow narrowing: sixteen 256ths of a floor pixel per screen pixel an update.</summary>
    private static FloorZoomStep SlowNarrowing { get; } = FloorZoomStep.FromTwoHundredFiftySixths(16);

    /// <summary>The zoom curve's fast narrowing: 32 256ths of a floor pixel per screen pixel an update.</summary>
    private static FloorZoomStep FastNarrowing { get; } = FloorZoomStep.FromTwoHundredFiftySixths(32);

    /// <summary>
    /// The getaway's zoom, 112 updates: it holds eight floor pixels per screen pixel, then narrows slowly, then fast, with
    /// two held updates between its fast ramps, to a last slow ramp that ends held at an eighth of a floor pixel.
    /// </summary>
    internal static IReadOnlyList<CeresRidleyGetawayZoomStretch> ZoomCurve { get; } =
    [
        CeresRidleyGetawayZoomStretch.Hold(FloorZoom.FromTwoHundredFiftySixths(2048), updates: 32),
        CeresRidleyGetawayZoomStretch.Narrowing(FloorZoom.FromTwoHundredFiftySixths(2032), SlowNarrowing, updates: 16),
        CeresRidleyGetawayZoomStretch.Narrowing(FloorZoom.FromTwoHundredFiftySixths(1760), FastNarrowing, updates: 22),
        CeresRidleyGetawayZoomStretch.Hold(FloorZoom.FromTwoHundredFiftySixths(1072), updates: 1),
        CeresRidleyGetawayZoomStretch.Narrowing(FloorZoom.FromTwoHundredFiftySixths(1024), FastNarrowing, updates: 15),
        CeresRidleyGetawayZoomStretch.Hold(FloorZoom.FromTwoHundredFiftySixths(560), updates: 1),
        CeresRidleyGetawayZoomStretch.Narrowing(FloorZoom.FromTwoHundredFiftySixths(512), FastNarrowing, updates: 9),
        CeresRidleyGetawayZoomStretch.Narrowing(FloorZoom.FromTwoHundredFiftySixths(240), SlowNarrowing, updates: 14),
        CeresRidleyGetawayZoomStretch.Hold(FloorZoom.FromTwoHundredFiftySixths(32), updates: 2),
    ];

    /// <summary>
    /// The getaway's drift, 112 updates: its vertical drift eases from six pixels an update to none over its first 80
    /// updates, then reverses and grows ever faster, while its horizontal drift holds at a pixel an update, then reverses
    /// and grows with it.
    /// </summary>
    internal static IReadOnlyList<CeresRidleyGetawayDriftStretch> DriftCurve { get; } =
    [
        new(new(-1, -6), Updates: 12),
        new(new(-1, -4), Updates: 10),
        new(new(-1, -2), Updates: 7),
        new(new(-1, -1), Updates: 16),
        new(new(-1, 0), Updates: 35),
        new(new(0, 1), Updates: 8),
        new(new(1, 2), Updates: 8),
        new(new(2, 3), Updates: 1),
        new(new(2, 4), Updates: 1),
        new(new(3, 6), Updates: 1),
        new(new(3, 8), Updates: 1),
        new(new(4, 10), Updates: 1),
        new(new(4, 12), Updates: 1),
        new(new(5, 14), Updates: 1),
        new(new(5, 16), Updates: 1),
        new(new(8, 20), Updates: 1),
        new(new(12, 24), Updates: 1),
        new(new(16, 44), Updates: 1),
        new(new(20, 48), Updates: 1),
        new(new(32, 128), Updates: 1),
        new(new(32, 256), Updates: 3),
    ];
}
