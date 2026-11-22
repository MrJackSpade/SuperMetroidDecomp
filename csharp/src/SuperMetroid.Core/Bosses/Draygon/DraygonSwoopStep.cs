namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// A phase of a swoop Draygon flies along a planned path: the leftward setup, which flies the path the rightward one
/// planned, and the descent, bottom and climb of either side's swoop.
/// </summary>
/// <param name="phase">The phase the step runs.</param>
/// <param name="path">The path the rightward setup planned.</param>
internal abstract partial class DraygonSwoopStep(DraygonPhase phase, DraygonSwoopPath path) : DraygonPhaseStep(phase)
{
    /// <summary>The constructor's <c>phase</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly DraygonPhase phase = phase;

    /// <summary>
    /// The swoop's path: the heights from the swoop's lowest point up toward the reset height and the height the rightward
    /// setup started it at.
    /// </summary>
    internal DraygonSwoopPath Path { get; } = path;
}
