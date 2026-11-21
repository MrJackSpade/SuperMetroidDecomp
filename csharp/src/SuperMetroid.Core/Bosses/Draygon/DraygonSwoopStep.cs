using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// A phase of a swoop Draygon flies along a planned path: the leftward setup, which flies the path the rightward one
/// planned, and the descent, bottom and climb of either side's swoop.
/// </summary>
internal sealed class DraygonSwoopStep : DraygonPhaseStep
{
    /// <summary>What the phase does each update along the path.</summary>
    private readonly Action<DraygonBehavior, DraygonEnemyState, SamusState, int, DraygonSwoopPath> _run;

    /// <summary>Builds the phase over the path it flies.</summary>
    /// <param name="phase">The phase the step runs.</param>
    /// <param name="path">The path the rightward setup planned.</param>
    /// <param name="run">What the phase does each update along the path.</param>
    internal DraygonSwoopStep(DraygonPhase phase, DraygonSwoopPath path,
        Action<DraygonBehavior, DraygonEnemyState, SamusState, int, DraygonSwoopPath> run)
        : base(phase)
    {
        Path = path;
        _run = run;
    }

    /// <summary>
    /// The swoop's path: the heights from the swoop's lowest point up toward the reset height and the height the rightward
    /// setup started it at.
    /// </summary>
    internal DraygonSwoopPath Path { get; }

    /// <inheritdoc/>
    internal override void Run(DraygonBehavior draygon, DraygonEnemyState state, SamusState samus, int frameCounter) =>
        _run(draygon, state, samus, frameCounter, Path);
}
