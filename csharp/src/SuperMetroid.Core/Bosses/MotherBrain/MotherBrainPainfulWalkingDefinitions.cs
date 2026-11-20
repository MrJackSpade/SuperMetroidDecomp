namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The stages of Mother Brain's drained stagger, forward and back in turn, each pair sharing its timing.</summary>
internal static class MotherBrainPainfulWalkingDefinitions
{
    /// <summary>Her fastest stagger forward: the neck turns five steps an update, then she pauses, the shortest of her pauses.</summary>
    private static readonly MotherBrainStaggerStage FastestForward = new(MotherBrainWalkSpeed.ReallyFast, RotationAngle.FromSteps(5), 16, MotherBrainStaggerDirection.Forward);

    /// <summary>Her fastest stagger back, timed as the stagger forward.</summary>
    private static readonly MotherBrainStaggerStage FastestBack = new(MotherBrainWalkSpeed.ReallyFast, RotationAngle.FromSteps(5), 16, MotherBrainStaggerDirection.Back);

    /// <summary>Her middling stagger forward: the neck turns two steps an update, then she pauses longer.</summary>
    private static readonly MotherBrainStaggerStage MiddlingForward = new(MotherBrainWalkSpeed.Medium, RotationAngle.FromSteps(2), 32, MotherBrainStaggerDirection.Forward);

    /// <summary>Her middling stagger back, timed as the stagger forward.</summary>
    private static readonly MotherBrainStaggerStage MiddlingBack = new(MotherBrainWalkSpeed.Medium, RotationAngle.FromSteps(2), 32, MotherBrainStaggerDirection.Back);

    /// <summary>Her slow stagger forward: the neck turns three quarters of a step an update, then she pauses longer still.</summary>
    private static readonly MotherBrainStaggerStage SlowForward = new(MotherBrainWalkSpeed.Slow, RotationAngle.FromStepFraction(3, 4), 48, MotherBrainStaggerDirection.Forward);

    /// <summary>Her slow stagger back, timed as the stagger forward.</summary>
    private static readonly MotherBrainStaggerStage SlowBack = new(MotherBrainWalkSpeed.Slow, RotationAngle.FromStepFraction(3, 4), 48, MotherBrainStaggerDirection.Back);

    /// <summary>Her slowest stagger back, the last.</summary>
    private static readonly MotherBrainStaggerStage SlowestBack = new(MotherBrainWalkSpeed.ReallySlow, RotationAngle.FromStepFraction(1, 4), 64, MotherBrainStaggerDirection.Back);

    /// <summary>The stage her rainbow beam runs out as she begins: her slowest stagger forward.</summary>
    private static MotherBrainStaggerStage BeamRunsOut { get; } = new(MotherBrainWalkSpeed.ReallySlow, RotationAngle.FromStepFraction(1, 4), 64, MotherBrainStaggerDirection.Forward,
        beamRunsOut: true);

    /// <summary>The stagger's stages in the order she walks them, forward and back in turn.</summary>
    internal static IReadOnlyList<MotherBrainStaggerStage> Stages { get; } =
        [FastestForward, FastestBack, MiddlingForward, MiddlingBack, SlowForward, SlowBack, BeamRunsOut, SlowestBack];
}
