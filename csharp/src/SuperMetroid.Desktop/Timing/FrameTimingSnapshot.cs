using System.Globalization;

namespace SuperMetroid.Desktop.Timing;

/// <summary>
/// One published interval of host-side timing data: completed game frames, their cost and the late frames discarded.
/// The renderer's own timings are reported beside it.
/// </summary>
public readonly record struct FrameTimingSnapshot(
    double UpdatesPerSecond,
    double AverageUpdateMilliseconds,
    double WorstUpdateMilliseconds,
    double LateFrames)
{
    /// <summary>Compact text intended for the fixed-width gameplay toolbar.</summary>
    public string ToToolbarText() => string.Create(
        CultureInfo.InvariantCulture,
        $"upd {UpdatesPerSecond,4:F1} | " +
        $"step {AverageUpdateMilliseconds:F1}/{WorstUpdateMilliseconds:F1} ms | " +
        $"late {LateFrames:F1}");

    /// <summary>Expanded explanation retained in the status tooltip.</summary>
    public string ToDiagnosticText() => string.Create(
        CultureInfo.InvariantCulture,
        $"Game updates {UpdatesPerSecond:F2} per second; " +
        $"update average/worst {AverageUpdateMilliseconds:F2}/{WorstUpdateMilliseconds:F2} ms; " +
        $"discarded late frames {LateFrames:F2}");
}
