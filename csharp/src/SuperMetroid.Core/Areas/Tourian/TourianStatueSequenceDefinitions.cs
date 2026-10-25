namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four-boss statue sequence's timing and shaking (#1415): how long the statue waits once the last boss is
/// released, how it lowers, and the earthquakes the descent and each release start.
/// </summary>
public static class TourianStatueSequenceDefinitions
{
    /// <summary>The updates the statue waits, after the last release ends, before it starts to lower.</summary>
    public const int DescentDelay = 300;

    /// <summary>How far the statue lowers each update: a quarter of a pixel, as a 16.16 distance.</summary>
    public static readonly int DescentStep = SixteenSixteen.FromWhole(1) / 4;

    /// <summary>The pixels the statue lowers before the descent completes.</summary>
    public const int DescentDistance = 240;

    /// <summary>The earthquake the descent shakes the room with: type thirteen.</summary>
    public const ushort DescentEarthquakeType = 13;

    /// <summary>The timer bit the descent keeps setting, so the shake lasts at least 32 updates past its end.</summary>
    public const ushort DescentEarthquakeTimer = 32;

    /// <summary>The earthquake a release shakes the room with: type one.</summary>
    public const ushort ReleaseEarthquakeType = 1;

    /// <summary>The timer bit a release sets, so the shake lasts at least 32 updates.</summary>
    public const ushort ReleaseEarthquakeTimer = 32;
}
