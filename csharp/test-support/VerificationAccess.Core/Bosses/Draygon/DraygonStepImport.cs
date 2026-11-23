namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The step a recorded Draygon phase stands for (#2554): production holds his phase as the step that runs it, a swoop's
/// with the path it flies and a goop pass's with the swoop setup it leads into, as his swoops choose them
/// (<see cref="DraygonSwoops"/>): a leftward goop pass leads into the rightward setup, a rightward one into the leftward
/// swoop over the path last planned.
/// </summary>
internal static class DraygonStepImport
{
    /// <summary>The step of <paramref name="phase"/> on a pass travelling <paramref name="pass"/> over <paramref name="path"/>.</summary>
    /// <param name="phase">The recorded phase.</param>
    /// <param name="pass">The way the recorded swoop or goop pass travels.</param>
    /// <param name="path">The recorded swoop path, or none before he plans his first swoop.</param>
    /// <exception cref="InvalidDataException">The phase flies a path the recording holds none of.</exception>
    internal static DraygonPhaseStep StepOf(DraygonPhase phase, HorizontalDirection pass, DraygonSwoopPath? path) => phase switch
    {
        DraygonPhase.IntroInitialDelay => DraygonPhaseSteps.IntroInitialDelay(),
        DraygonPhase.IntroDance => DraygonPhaseSteps.IntroDance(),
        DraygonPhase.SwoopSetup => pass == HorizontalDirection.Left ? DraygonPhaseSteps.LeftSwoopSetup(Path(phase, path)) : DraygonPhaseSteps.RightSwoopSetup(),
        DraygonPhase.SwoopDescending => DraygonPhaseSteps.SwoopDescending(Path(phase, path)),
        DraygonPhase.SwoopApex => DraygonPhaseSteps.SwoopApex(Path(phase, path)),
        DraygonPhase.SwoopAscending => DraygonPhaseSteps.SwoopAscending(Path(phase, path)),
        DraygonPhase.GoopSetup => DraygonPhaseSteps.GoopSetup(GoopExit(pass, path)),
        DraygonPhase.GoopApproach => DraygonPhaseSteps.GoopApproach(GoopExit(pass, path)),
        DraygonPhase.GoopTail => DraygonPhaseSteps.GoopTail(GoopExit(pass, path)),
        DraygonPhase.GoopRecovery => DraygonPhaseSteps.GoopRecovery(GoopExit(pass, path)),
        DraygonPhase.TryGrabSamus => DraygonPhaseSteps.TryGrabSamus(),
        DraygonPhase.RepelGrapple => DraygonPhaseSteps.RepelGrapple(),
        DraygonPhase.CarrySamus => DraygonPhaseSteps.CarrySamus(),
        DraygonPhase.FlailWithSamus => DraygonPhaseSteps.FlailWithSamus(),
        DraygonPhase.BeginTailWhipWithSamus => DraygonPhaseSteps.BeginTailWhipWithSamus(),
        DraygonPhase.TailWhipWithSamus => DraygonPhaseSteps.TailWhipWithSamus(),
        DraygonPhase.FinalTailWhips => DraygonPhaseSteps.FinalTailWhips(),
        DraygonPhase.FinalTailWhipsWait => DraygonPhaseSteps.FinalTailWhipsWait(),
        DraygonPhase.ReleaseSamus => DraygonPhaseSteps.ReleaseSamus(),
        DraygonPhase.FlyStraightUp => DraygonPhaseSteps.FlyStraightUp(),
        DraygonPhase.Dying => DraygonPhaseSteps.Dying(),
        DraygonPhase.DyingWaitForEvirs => DraygonPhaseSteps.DyingWaitForEvirs(),
        DraygonPhase.DyingFinish => DraygonPhaseSteps.DyingFinish(),
        _ => throw new InvalidDataException($"Undefined recorded {nameof(DraygonPhase)} {phase}."),
    };

    /// <summary>The swoop setup a goop pass travelling <paramref name="pass"/> leads into.</summary>
    private static DraygonPhaseStep GoopExit(HorizontalDirection pass, DraygonSwoopPath? path) =>
        pass == HorizontalDirection.Left ? DraygonPhaseSteps.RightSwoopSetup() : DraygonPhaseSteps.LeftSwoopSetup(Path(DraygonPhase.GoopSetup, path));

    /// <summary>The path a recorded phase flies.</summary>
    private static DraygonSwoopPath Path(DraygonPhase phase, DraygonSwoopPath? path) =>
        path ?? throw new InvalidDataException($"A recorded Draygon {phase} flies a swoop path the recording holds none of.");
}
