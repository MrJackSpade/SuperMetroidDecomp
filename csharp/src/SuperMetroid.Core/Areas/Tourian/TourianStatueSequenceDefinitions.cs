using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four-boss statue sequence's timing and shaking (#1415): how long the statue waits once the last boss is
/// released, how it lowers, and the earthquakes the descent and each release start.
/// </summary>
public static class TourianStatueSequenceDefinitions
{
    /// <summary>The updates the statue waits, after the last release ends, before it starts to lower.</summary>
    public const int DescentDelay = 300;

    /// <summary>How far the statue lowers each update: a quarter of a pixel.</summary>
    public static readonly SignedSixteenSixteen DescentStep = SignedSixteenSixteen.FromSixteenthsOfPixel(4);

    /// <summary>The pixels the statue lowers before the descent completes.</summary>
    public const int DescentDistance = 240;

    /// <summary>
    /// The medium up-and-down earthquake, shaking both layers, the descent sustains every update it runs
    /// (<see cref="SuperMetroid.Core.Fx.RoomEarthquake.Sustain"/>), so the shake lasts past its end.
    /// </summary>
    public static readonly EarthquakeShake DescentEarthquake =
        new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundAndBackground);

    /// <summary>The light up-and-down foreground earthquake a release sustains.</summary>
    public static readonly EarthquakeShake ReleaseEarthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.Foreground);
}
