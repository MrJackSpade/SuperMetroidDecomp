
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The steps and ends of a Dachora's glow cycles (#1544): its speed glow while it runs at full speed and its stored-shine glow.</summary>
internal static class DachoraGlowCycleDefinitions
{
    /// <summary>One step of the speed glow: the next frame, sixteen updates later.</summary>
    internal static DachoraGlowCycle SpeedStep { get; } = DachoraGlowCycle.At(1, 16);

    /// <summary>The speed glow restarts once a step brings its cycle to frame four with sixteen updates left.</summary>
    internal static DachoraGlowCycle SpeedEnd { get; } = DachoraGlowCycle.At(4, 16);

    /// <summary>
    /// Where the speed glow restarts: its brightest frame, a step away, so from then on it shows that frame at each step
    /// instead of starting over from frame zero.
    /// </summary>
    internal static DachoraGlowCycle SpeedRestart { get; } = DachoraGlowCycle.At(3, 16);

    /// <summary>One step of the shine glow: the next frame.</summary>
    internal static DachoraGlowCycle ShineStep { get; } = DachoraGlowCycle.At(1, 0);

    /// <summary>The shine glow returns to frame zero once a step brings its cycle to frame four.</summary>
    internal static DachoraGlowCycle ShineEnd { get; } = DachoraGlowCycle.At(4, 0);

    /// <summary>
    /// Where a run starts the speed glow: frame zero, one update away. The first update the run spends at full speed finds
    /// the cycle still there, requests the Speed Booster sound and counts the cycle out to its first frame.
    /// </summary>
    internal static DachoraGlowCycle RunStart { get; } = DachoraGlowCycle.At(0, 1);

    /// <summary>A cleared glow: frame zero with no updates left, as a stop, a charge or a fall leaves it.</summary>
    internal static DachoraGlowCycle Cleared { get; } = DachoraGlowCycle.At(0, 0);
}
