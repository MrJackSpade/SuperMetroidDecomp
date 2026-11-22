namespace SuperMetroid.Game.Steps;

/// <summary>A goop step, which hands its phase to its base and keeps it for its own rebuild.</summary>
/// <param name="phase">The phase the step runs.</param>
/// <param name="exit">Where the step leads.</param>
internal sealed class GoopStep(int phase, int exit) : Step(phase)
{
    /// <summary>The phase the step was made with, which it keeps so it can be made again from what it holds.</summary>
    private readonly int phase = phase;

    /// <summary>Where the step leads.</summary>
    internal int Exit { get; } = exit;
}
