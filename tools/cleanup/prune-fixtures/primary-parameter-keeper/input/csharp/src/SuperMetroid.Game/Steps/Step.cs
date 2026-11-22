namespace SuperMetroid.Game.Steps;

/// <summary>A step of a phase, which production reads the phase of.</summary>
/// <param name="phase">The phase the step runs.</param>
internal abstract class Step(int phase)
{
    /// <summary>The phase the step runs.</summary>
    internal int Phase { get; } = phase;
}
