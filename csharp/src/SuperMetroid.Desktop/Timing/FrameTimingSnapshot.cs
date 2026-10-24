using System.Globalization;

namespace SuperMetroid.Desktop.Timing;

/// <summary>
/// One published interval of host-side timing data: completed game frames, their cost and the late frames discarded.
/// The renderer's own timings are reported beside it.
/// </summary>
public readonly record struct FrameTimingSnapshot(
    double EmulatedFramesPerSecond,
    double AverageEmulationMilliseconds,
    double WorstEmulationMilliseconds,
    double LateFrames)
{
    /// <summary>Compact text intended for the fixed-width gameplay toolbar.</summary>
    public string ToToolbarText() => string.Create(
        CultureInfo.InvariantCulture,
        $"emu {EmulatedFramesPerSecond,4:F1} | " +
        $"step {AverageEmulationMilliseconds:F1}/{WorstEmulationMilliseconds:F1} ms | " +
        $"late {LateFrames:F1}");

    /// <summary>Expanded explanation retained in the status tooltip.</summary>
    public string ToDiagnosticText() => string.Create(
        CultureInfo.InvariantCulture,
        $"Emulation {EmulatedFramesPerSecond:F2} fps; " +
        $"emulation frame average/worst {AverageEmulationMilliseconds:F2}/{WorstEmulationMilliseconds:F2} ms; " +
        $"discarded late frames {LateFrames:F2}");
}
