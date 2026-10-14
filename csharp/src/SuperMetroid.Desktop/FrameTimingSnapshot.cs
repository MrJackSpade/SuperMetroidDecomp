using System.Globalization;

namespace SuperMetroid.Desktop;

/// <summary>
/// One published interval of host-side timing data. Emulated FPS measures completed
/// cartridge frames; paint FPS measures completed WinForms canvas paints. Keeping the two
/// rates separate makes a slow translation distinguishable from a slow presentation path.
/// </summary>
public readonly record struct FrameTimingSnapshot(
    double EmulatedFramesPerSecond,
    double PaintedFramesPerSecond,
    double AverageEmulationMilliseconds,
    double WorstEmulationMilliseconds,
    double AveragePaintMilliseconds,
    double WorstPaintMilliseconds,
    double LateFrames)
{
    /// <summary>Compact text intended for the fixed-width gameplay toolbar.</summary>
    public string ToToolbarText() => string.Create(
        CultureInfo.InvariantCulture,
        $"emu {EmulatedFramesPerSecond,4:F1} | paint {PaintedFramesPerSecond,4:F1} | " +
        $"step {AverageEmulationMilliseconds:F1}/{WorstEmulationMilliseconds:F1} ms | " +
        $"late {LateFrames:F1}");

    /// <summary>Expanded explanation retained in the status tooltip.</summary>
    public string ToDiagnosticText() => string.Create(
        CultureInfo.InvariantCulture,
        $"Emulation {EmulatedFramesPerSecond:F2} fps, canvas paint {PaintedFramesPerSecond:F2} fps; " +
        $"emulation frame average/worst {AverageEmulationMilliseconds:F2}/{WorstEmulationMilliseconds:F2} ms; " +
        $"paint average/worst {AveragePaintMilliseconds:F2}/{WorstPaintMilliseconds:F2} ms; " +
        $"discarded late frames {LateFrames:F2}");
}
