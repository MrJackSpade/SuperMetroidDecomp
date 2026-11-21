namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's phases (#2554), each the step his body runs while in it: the phases that run from his state alone, and the
/// swoop and goop phases built over the path or the setup they hold.
/// </summary>
internal static class DraygonPhaseSteps
{
    /// <summary>The fight's opening delay, while the dancing Evirs spawn.</summary>
    internal static DraygonPhaseStep IntroInitialDelay { get; } = new DraygonPlainStep(DraygonPhase.IntroInitialDelay,
        (draygon, state, samus, frameCounter) => draygon.Intro.RunDraygonIntroInitialDelay(state, samus, frameCounter));

    /// <summary>The Evirs' opening dance.</summary>
    internal static DraygonPhaseStep IntroDance { get; } = new DraygonPlainStep(DraygonPhase.IntroDance,
        (draygon, state, samus, frameCounter) => draygon.Intro.RunDraygonIntroDance(state, samus, frameCounter));

    /// <summary>The rightward swoop's setup, which plans the swoop's path.</summary>
    internal static DraygonPhaseStep RightSwoopSetup { get; } = new DraygonPlainStep(DraygonPhase.SwoopSetup,
        (draygon, state, samus, frameCounter) => draygon.Swoops.SetupDraygonRightSwoop(state, samus, frameCounter));

    /// <summary>The leftward swoop's setup, flying <paramref name="path"/>, the path the rightward setup last planned.</summary>
    /// <param name="path">The path last planned.</param>
    internal static DraygonPhaseStep LeftSwoopSetup(DraygonSwoopPath path) => new DraygonSwoopStep(DraygonPhase.SwoopSetup, path,
        (draygon, state, samus, frameCounter, flown) => draygon.Swoops.SetupDraygonLeftSwoop(state, samus, frameCounter, flown));

    /// <summary>The swoop's descent along <paramref name="path"/>.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep SwoopDescending(DraygonSwoopPath path) => new DraygonSwoopStep(DraygonPhase.SwoopDescending,
        path, (draygon, state, samus, frameCounter, flown) =>
            draygon.Swoops.DescendDraygonSwoop(state, samus, frameCounter, DraygonBehavior.SwoopSideOf(state), flown));

    /// <summary>The swoop's bottom, where it aims its climb along <paramref name="path"/> at the side it flies to.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep SwoopApex(DraygonSwoopPath path) => new DraygonSwoopStep(DraygonPhase.SwoopApex, path,
        (draygon, state, samus, frameCounter, flown) => draygon.Swoops.CrossDraygonSwoopApex(state, samus, frameCounter, flown));

    /// <summary>The swoop's climb along <paramref name="path"/>.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep SwoopAscending(DraygonSwoopPath path) => new DraygonSwoopStep(DraygonPhase.SwoopAscending,
        path, (draygon, state, samus, frameCounter, flown) =>
            draygon.Swoops.AscendDraygonSwoop(state, samus, frameCounter, DraygonBehavior.SwoopSideOf(state), flown));

    /// <summary>A goop pass's setup, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopSetup(DraygonPhaseStep exit) => new DraygonGoopStep(DraygonPhase.GoopSetup, exit,
        (draygon, state, samus, frameCounter, next) =>
            draygon.Goop.SetupDraygonGoopPass(state, state.PassDirection, samus, frameCounter, next));

    /// <summary>A goop pass's approach toward Samus, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopApproach(DraygonPhaseStep exit) => new DraygonGoopStep(DraygonPhase.GoopApproach, exit,
        (draygon, state, samus, frameCounter, next) => draygon.Goop.ApproachSamusForDraygonGoop(state, samus, frameCounter, next));

    /// <summary>A goop pass's firing, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopTail(DraygonPhaseStep exit) => new DraygonGoopStep(DraygonPhase.GoopTail, exit,
        (draygon, state, samus, frameCounter, next) =>
            draygon.Goop.FireDraygonGoop(state, state.PassDirection, samus, frameCounter, next));

    /// <summary>A goop pass's way off the screen, leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopRecovery(DraygonPhaseStep exit) => new DraygonGoopStep(DraygonPhase.GoopRecovery, exit,
        (draygon, state, samus, _, next) => draygon.Goop.ExitDraygonGoopPass(state, state.PassDirection, samus, next));

    /// <summary>The chase that tries to grab a slowed Samus.</summary>
    internal static DraygonPhaseStep TryGrabSamus { get; } = new DraygonPlainStep(DraygonPhase.TryGrabSamus,
        (draygon, state, samus, frameCounter) => draygon.Grab.ChaseAndTryToGrabSamus(state, samus, frameCounter));

    /// <summary>The recoil from Samus's grapple.</summary>
    internal static DraygonPhaseStep RepelGrapple { get; } = new DraygonPlainStep(DraygonPhase.RepelGrapple,
        (_, state, samus, _) => DraygonGrab.RepelDraygonWithGrapple(state, samus));

    /// <summary>The carry of a grabbed Samus to the spiral.</summary>
    internal static DraygonPhaseStep CarrySamus { get; } = new DraygonPlainStep(DraygonPhase.CarrySamus,
        (draygon, state, samus, _) => draygon.Grab.CarrySamusToDraygonSpiral(state, samus));

    /// <summary>The spiral with Samus held.</summary>
    internal static DraygonPhaseStep FlailWithSamus { get; } = new DraygonPlainStep(DraygonPhase.FlailWithSamus,
        (draygon, state, samus, _) => draygon.Grab.CarrySamusInDraygonSpiral(state, samus));

    /// <summary>The start of a tail whip with Samus held.</summary>
    internal static DraygonPhaseStep BeginTailWhipWithSamus { get; } = new DraygonPlainStep(DraygonPhase.BeginTailWhipWithSamus,
        (draygon, state, samus, _) => draygon.Grab.BeginDraygonTailWhip(state, samus));

    /// <summary>A tail whip with Samus held.</summary>
    internal static DraygonPhaseStep TailWhipWithSamus { get; } = new DraygonPlainStep(DraygonPhase.TailWhipWithSamus,
        (draygon, state, samus, _) => draygon.Grab.RunDraygonTailWhip(state, samus));

    /// <summary>The start of the final tail whips.</summary>
    internal static DraygonPhaseStep FinalTailWhips { get; } = new DraygonPlainStep(DraygonPhase.FinalTailWhips,
        (draygon, state, samus, _) => draygon.Grab.BeginDraygonFinalTailWhips(state, samus));

    /// <summary>The wait through the final tail whips.</summary>
    internal static DraygonPhaseStep FinalTailWhipsWait { get; } = new DraygonPlainStep(DraygonPhase.FinalTailWhipsWait,
        (draygon, state, samus, _) => draygon.Grab.WaitForDraygonFinalTailWhips(state, samus));

    /// <summary>The release of a held Samus.</summary>
    internal static DraygonPhaseStep ReleaseSamus { get; } = new DraygonPlainStep(DraygonPhase.ReleaseSamus,
        (draygon, state, samus, _) => draygon.Grab.ReleaseSamusFromDraygon(state, samus));

    /// <summary>The climb straight up and out of the room, back to the rightward swoop's setup.</summary>
    internal static DraygonPhaseStep FlyStraightUp { get; } = new DraygonPlainStep(DraygonPhase.FlyStraightUp,
        (draygon, state, samus, frameCounter) => draygon.Swoops.FlyDraygonStraightUp(state, samus, frameCounter));

    /// <summary>The dying body's drift to its burial point.</summary>
    internal static DraygonPhaseStep Dying { get; } = new DraygonPlainStep(DraygonPhase.Dying,
        (draygon, state, _, _) => draygon.Death.DriftDyingDraygonToBurialPoint(state));

    /// <summary>The dying body's wait for its burial Evirs.</summary>
    internal static DraygonPhaseStep DyingWaitForEvirs { get; } = new DraygonPlainStep(DraygonPhase.DyingWaitForEvirs,
        (draygon, state, _, frameCounter) => draygon.Death.WaitForDraygonBurialEvirs(state, frameCounter));

    /// <summary>The dying body's sink below the room.</summary>
    internal static DraygonPhaseStep DyingFinish { get; } = new DraygonPlainStep(DraygonPhase.DyingFinish,
        (draygon, state, samus, frameCounter) => draygon.Death.SinkDraygonBelowTheRoom(state, frameCounter, samus));
}
