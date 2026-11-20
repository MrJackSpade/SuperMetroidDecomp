namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The stages of Mother Brain's drained stagger, forward and back in turn, each pair sharing its timing.</summary>
internal static class MotherBrainPainfulWalkingDefinitions
{
    /// <summary>Her fastest stagger, forward and back: the neck turns five steps an update, then she pauses, the shortest of her pauses.</summary>
    private static readonly MotherBrainStaggerStage Fastest = new(MotherBrainWalkSpeed.ReallyFast, RotationAngle.FromSteps(5), 16);

    /// <summary>Her middling stagger: the neck turns two steps an update, then she pauses longer.</summary>
    private static readonly MotherBrainStaggerStage Middling = new(MotherBrainWalkSpeed.Medium, RotationAngle.FromSteps(2), 32);

    /// <summary>Her slow stagger: the neck turns three quarters of a step an update, then she pauses longer still.</summary>
    private static readonly MotherBrainStaggerStage Slow = new(MotherBrainWalkSpeed.Slow, RotationAngle.FromStepFraction(3, 4), 48);

    /// <summary>Her slowest stagger, the last pair.</summary>
    private static readonly MotherBrainStaggerStage Slowest = new(MotherBrainWalkSpeed.ReallySlow, RotationAngle.FromStepFraction(1, 4), 64);

    /// <summary>The stagger's stages in the order she walks them, forward first.</summary>
    internal static IReadOnlyList<MotherBrainStaggerStage> Stages { get; } =
        [Fastest, Fastest, Middling, Middling, Slow, Slow, Slowest, Slowest];

    /// <summary>The place in <see cref="Stages"/> of the stage her rainbow beam runs out as she begins: her slowest stagger forward.</summary>
    internal const int BeamRunsOutStage = 6;
}
