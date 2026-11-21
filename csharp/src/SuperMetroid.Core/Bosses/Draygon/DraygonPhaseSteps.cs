namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's phases (#2554), each the step his body runs while in it: the phases that run from his state alone, which are
/// one step each, and the swoop and goop phases, built over the path or the setup they hold.
/// </summary>
internal static class DraygonPhaseSteps
{
    /// <summary>The fight's opening delay, while the dancing Evirs spawn.</summary>
    internal static DraygonPhaseStep IntroInitialDelay() => IntroInitialDelayStep.Instance;

    /// <summary>The Evirs' opening dance.</summary>
    internal static DraygonPhaseStep IntroDance() => IntroDanceStep.Instance;

    /// <summary>The rightward swoop's setup, which plans the swoop's path.</summary>
    internal static DraygonPhaseStep RightSwoopSetup() => RightSwoopSetupStep.Instance;

    /// <summary>The chase that tries to grab a slowed Samus.</summary>
    internal static DraygonPhaseStep TryGrabSamus() => TryGrabSamusStep.Instance;

    /// <summary>The recoil from Samus's grapple.</summary>
    internal static DraygonPhaseStep RepelGrapple() => RepelGrappleStep.Instance;

    /// <summary>The carry of a grabbed Samus to the spiral.</summary>
    internal static DraygonPhaseStep CarrySamus() => CarrySamusStep.Instance;

    /// <summary>The spiral with Samus held.</summary>
    internal static DraygonPhaseStep FlailWithSamus() => FlailWithSamusStep.Instance;

    /// <summary>The start of a tail whip with Samus held.</summary>
    internal static DraygonPhaseStep BeginTailWhipWithSamus() => BeginTailWhipWithSamusStep.Instance;

    /// <summary>A tail whip with Samus held.</summary>
    internal static DraygonPhaseStep TailWhipWithSamus() => TailWhipWithSamusStep.Instance;

    /// <summary>The start of the final tail whips.</summary>
    internal static DraygonPhaseStep FinalTailWhips() => FinalTailWhipsStep.Instance;

    /// <summary>The wait through the final tail whips.</summary>
    internal static DraygonPhaseStep FinalTailWhipsWait() => FinalTailWhipsWaitStep.Instance;

    /// <summary>The release of a held Samus.</summary>
    internal static DraygonPhaseStep ReleaseSamus() => ReleaseSamusStep.Instance;

    /// <summary>The climb straight up and out of the room, back to the rightward swoop's setup.</summary>
    internal static DraygonPhaseStep FlyStraightUp() => FlyStraightUpStep.Instance;

    /// <summary>The dying body's drift to its burial point.</summary>
    internal static DraygonPhaseStep Dying() => DyingStep.Instance;

    /// <summary>The dying body's wait for its burial Evirs.</summary>
    internal static DraygonPhaseStep DyingWaitForEvirs() => DyingWaitForEvirsStep.Instance;

    /// <summary>The dying body's sink below the room.</summary>
    internal static DraygonPhaseStep DyingFinish() => DyingFinishStep.Instance;

    /// <summary>The leftward swoop's setup, flying the path the rightward setup last planned.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep LeftSwoopSetup(DraygonSwoopPath path) => new LeftSwoopSetupStep(path);

    /// <summary>The swoop's descent along its path.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep SwoopDescending(DraygonSwoopPath path) => new SwoopDescendingStep(path);

    /// <summary>The swoop's bottom, where it aims its climb along its path at the side it flies to.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep SwoopApex(DraygonSwoopPath path) => new SwoopApexStep(path);

    /// <summary>The swoop's climb along its path.</summary>
    /// <param name="path">The path the swoop flies.</param>
    internal static DraygonPhaseStep SwoopAscending(DraygonSwoopPath path) => new SwoopAscendingStep(path);

    /// <summary>A goop pass's setup, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopSetup(DraygonPhaseStep exit) => new GoopSetupStep(exit);

    /// <summary>A goop pass's approach toward Samus, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopApproach(DraygonPhaseStep exit) => new GoopApproachStep(exit);

    /// <summary>A goop pass's firing, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopTail(DraygonPhaseStep exit) => new GoopTailStep(exit);

    /// <summary>A goop pass's way off the screen, the pass leading into <paramref name="exit"/>.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    internal static DraygonPhaseStep GoopRecovery(DraygonPhaseStep exit) => new GoopRecoveryStep(exit);

    /// <summary>The fight's opening delay, while the dancing Evirs spawn.</summary>
    private sealed class IntroInitialDelayStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static IntroInitialDelayStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private IntroInitialDelayStep()
            : base(DraygonPhase.IntroInitialDelay)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Intro.RunDraygonIntroInitialDelay(update.State, update.Samus, update.FrameCounter);
    }

    /// <summary>The Evirs' opening dance.</summary>
    private sealed class IntroDanceStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static IntroDanceStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private IntroDanceStep()
            : base(DraygonPhase.IntroDance)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Intro.RunDraygonIntroDance(update.State, update.Samus, update.FrameCounter);
    }

    /// <summary>The rightward swoop's setup, which plans the swoop's path.</summary>
    private sealed class RightSwoopSetupStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static RightSwoopSetupStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private RightSwoopSetupStep()
            : base(DraygonPhase.SwoopSetup)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Swoops.SetupDraygonRightSwoop(update.State, update.Samus, update.FrameCounter);
    }

    /// <summary>The chase that tries to grab a slowed Samus.</summary>
    private sealed class TryGrabSamusStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static TryGrabSamusStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private TryGrabSamusStep()
            : base(DraygonPhase.TryGrabSamus)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.ChaseAndTryToGrabSamus(update.State, update.Samus, update.FrameCounter);
    }

    /// <summary>The recoil from Samus's grapple.</summary>
    private sealed class RepelGrappleStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static RepelGrappleStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private RepelGrappleStep()
            : base(DraygonPhase.RepelGrapple)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => DraygonGrab.RepelDraygonWithGrapple(update.State, update.Samus);
    }

    /// <summary>The carry of a grabbed Samus to the spiral.</summary>
    private sealed class CarrySamusStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static CarrySamusStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private CarrySamusStep()
            : base(DraygonPhase.CarrySamus)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.CarrySamusToDraygonSpiral(update.State, update.Samus);
    }

    /// <summary>The spiral with Samus held.</summary>
    private sealed class FlailWithSamusStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static FlailWithSamusStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private FlailWithSamusStep()
            : base(DraygonPhase.FlailWithSamus)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.CarrySamusInDraygonSpiral(update.State, update.Samus);
    }

    /// <summary>The start of a tail whip with Samus held.</summary>
    private sealed class BeginTailWhipWithSamusStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static BeginTailWhipWithSamusStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private BeginTailWhipWithSamusStep()
            : base(DraygonPhase.BeginTailWhipWithSamus)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.BeginDraygonTailWhip(update.State, update.Samus);
    }

    /// <summary>A tail whip with Samus held.</summary>
    private sealed class TailWhipWithSamusStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static TailWhipWithSamusStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private TailWhipWithSamusStep()
            : base(DraygonPhase.TailWhipWithSamus)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.RunDraygonTailWhip(update.State, update.Samus);
    }

    /// <summary>The start of the final tail whips.</summary>
    private sealed class FinalTailWhipsStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static FinalTailWhipsStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private FinalTailWhipsStep()
            : base(DraygonPhase.FinalTailWhips)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.BeginDraygonFinalTailWhips(update.State, update.Samus);
    }

    /// <summary>The wait through the final tail whips.</summary>
    private sealed class FinalTailWhipsWaitStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static FinalTailWhipsWaitStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private FinalTailWhipsWaitStep()
            : base(DraygonPhase.FinalTailWhipsWait)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.WaitForDraygonFinalTailWhips(update.State, update.Samus);
    }

    /// <summary>The release of a held Samus.</summary>
    private sealed class ReleaseSamusStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static ReleaseSamusStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private ReleaseSamusStep()
            : base(DraygonPhase.ReleaseSamus)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Grab.ReleaseSamusFromDraygon(update.State, update.Samus);
    }

    /// <summary>The climb straight up and out of the room, back to the rightward swoop's setup.</summary>
    private sealed class FlyStraightUpStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static FlyStraightUpStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private FlyStraightUpStep()
            : base(DraygonPhase.FlyStraightUp)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Swoops.FlyDraygonStraightUp(update.State, update.Samus, update.FrameCounter);
    }

    /// <summary>The dying body's drift to its burial point.</summary>
    private sealed class DyingStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static DyingStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private DyingStep()
            : base(DraygonPhase.Dying)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Death.DriftDyingDraygonToBurialPoint(update.State);
    }

    /// <summary>The dying body's wait for its burial Evirs.</summary>
    private sealed class DyingWaitForEvirsStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static DyingWaitForEvirsStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private DyingWaitForEvirsStep()
            : base(DraygonPhase.DyingWaitForEvirs)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Death.WaitForDraygonBurialEvirs(update.State, update.FrameCounter);
    }

    /// <summary>The dying body's sink below the room.</summary>
    private sealed class DyingFinishStep : DraygonPhaseStep
    {
        /// <summary>The step's only value.</summary>
        internal static DyingFinishStep Instance { get; } = new();

        /// <summary>Names the step by its phase.</summary>
        private DyingFinishStep()
            : base(DraygonPhase.DyingFinish)
        {
        }

        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Death.SinkDraygonBelowTheRoom(update.State, update.FrameCounter, update.Samus);
    }

    /// <summary>The leftward swoop's setup, flying the path the rightward setup last planned.</summary>
    /// <param name="path">The path the swoop flies.</param>
    private sealed class LeftSwoopSetupStep(DraygonSwoopPath path) : DraygonSwoopStep(DraygonPhase.SwoopSetup, path)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Swoops.SetupDraygonLeftSwoop(update.State, update.Samus, update.FrameCounter, Path);
    }

    /// <summary>The swoop's descent along its path.</summary>
    /// <param name="path">The path the swoop flies.</param>
    private sealed class SwoopDescendingStep(DraygonSwoopPath path) : DraygonSwoopStep(DraygonPhase.SwoopDescending, path)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Swoops.DescendDraygonSwoop(update.State, update.Samus, update.FrameCounter, DraygonBehavior.SwoopSideOf(update.State), Path);
    }

    /// <summary>The swoop's bottom, where it aims its climb along its path at the side it flies to.</summary>
    /// <param name="path">The path the swoop flies.</param>
    private sealed class SwoopApexStep(DraygonSwoopPath path) : DraygonSwoopStep(DraygonPhase.SwoopApex, path)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Swoops.CrossDraygonSwoopApex(update.State, update.Samus, update.FrameCounter, Path);
    }

    /// <summary>The swoop's climb along its path.</summary>
    /// <param name="path">The path the swoop flies.</param>
    private sealed class SwoopAscendingStep(DraygonSwoopPath path) : DraygonSwoopStep(DraygonPhase.SwoopAscending, path)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Swoops.AscendDraygonSwoop(update.State, update.Samus, update.FrameCounter, DraygonBehavior.SwoopSideOf(update.State), Path);
    }

    /// <summary>A goop pass's setup.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    private sealed class GoopSetupStep(DraygonPhaseStep exit) : DraygonGoopStep(DraygonPhase.GoopSetup, exit)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Goop.SetupDraygonGoopPass(update.State, update.State.PassDirection, update.Samus, update.FrameCounter, Exit);
    }

    /// <summary>A goop pass's approach toward Samus.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    private sealed class GoopApproachStep(DraygonPhaseStep exit) : DraygonGoopStep(DraygonPhase.GoopApproach, exit)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Goop.ApproachSamusForDraygonGoop(update.State, update.Samus, update.FrameCounter, Exit);
    }

    /// <summary>A goop pass's firing.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    private sealed class GoopTailStep(DraygonPhaseStep exit) : DraygonGoopStep(DraygonPhase.GoopTail, exit)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Goop.FireDraygonGoop(update.State, update.State.PassDirection, update.Samus, update.FrameCounter, Exit);
    }

    /// <summary>A goop pass's way off the screen.</summary>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    private sealed class GoopRecoveryStep(DraygonPhaseStep exit) : DraygonGoopStep(DraygonPhase.GoopRecovery, exit)
    {
        /// <inheritdoc/>
        internal override void Run(DraygonPhaseUpdate update) => update.Draygon.Goop.ExitDraygonGoopPass(update.State, update.State.PassDirection, update.Samus, Exit);
    }
}
