namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's third phase: recovering, setting up and fighting Samus after the baby cutscene.</summary>
internal sealed class MotherBrainThirdPhaseFight
{
    /// <summary>The late fight's own state, with the walking and posture helpers every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    /// <param name="state">The state every late-fight phase reads and writes.</param>
    /// <param name="walk">The walk credit of the encounter the sequence is attached to.</param>
    internal MotherBrainThirdPhaseFight(MotherBrainLateFightState state, MotherBrainWalkCounter walk)
    {
        _state = state;
        _walk = Ensure.NotNull(walk);
    }

    /// <summary>
    /// Mother Brain's phase-three walking.
    /// This is deliberately separate from the visible body animation: the walking
    /// decides what Mother Brain wants to do only while the animation-owned pose is standing.
    /// </summary>
    public MotherBrainPhase3WalkingPhase Phase3WalkingPhase { get; private set; } =
        MotherBrainPhase3WalkingPhase.Inactive;

    /// <summary>The walk credit of the encounter the sequence is attached to, which its walking and shot reactions also read and write.</summary>
    private readonly MotherBrainWalkCounter _walk;

    /// <summary>
    /// Mother Brain's walk credit as the sequence uses it. Ordinary projectiles subtract
    /// 256; Hyper Beam subtracts 266; the walking phase adds
    /// 32 whenever it gets a standing AI call.
    /// </summary>
    public MotherBrainWalkCredit Phase3WalkCounter
    {
        get => _walk.Credit;
        internal set => _walk.Credit = value;
    }

    /// <summary>Current phase-three walking target X in room pixels, within a pixel of the body's walking range.</summary>
    public int Phase3TargetXPosition { get; private set; }

    /// <summary>Mother Brain's phase-three neck movement.</summary>
    public MotherBrainPhase3NeckPhase Phase3NeckPhase { get; internal set; } =
        MotherBrainPhase3NeckPhase.Inactive;

    /// <summary>Countdown shared by the two phase-three recoil phases; it stops at zero.</summary>
    [IntegerQuantity("frames left in the recoil; the death sequence waits for the tenth")]
    public int Phase3NeckRecoilTimer { get; private set; }

    /// <summary>
    /// Whether phase-three attacks are suspended. Hyper Beam recoil sets it
    /// and the recoil timer clears it before the recovery pose begins.
    /// </summary>
    public bool Phase3DisableAttacks { get; private set; }

    /// <summary>Schedules phase-three inching and two retreat speeds only while the body is standing.</summary>
    private void StepPhase3Walk(MotherBrainRandomSample randomNumberSeed)
    {
        // The phase-three scheduler refuses to run the walking phase while the body animation advertises any
        // nonzero pose. This makes each request edge-triggered: the requested walk runs to
        // its standing cue before the scheduler is allowed to inspect its target again.
        if (Encounter.Pose != MotherBrainBodyPose.Standing)
            return;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter.IsEmpty)
                {
                    // Zero credit does not merely pause. The retreat chooses a point fourteen
                    // pixels left, starts the quick-retreat phase, and falls into it.
                    Phase3TargetXPosition = _state.BodyPosition.X - 14;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // The addition and comparison are unsigned here. A wrapped counter can therefore spend
                // more calls below 256; no integer widening or saturation belongs.
                Phase3WalkCounter = Phase3WalkCounter.AddCredit(32);
                if (Phase3WalkCounter.BelowStep)
                    return;

                Phase3TargetXPosition = _state.BodyPosition.X + 1;
                int forwardDelay = (randomNumberSeed.IsSet(MotherBrainRandomDefinitions.ForwardInchLongerDelay) ? 2 : 0) + 4;

                // The forward walk reports arrival only for a strictly overshot
                // target or the independent X 128 limit. Equality still requests a walk.
                bool reachedForwardTarget =
                    CoordinateAxis.Difference(Phase3TargetXPosition, _state.BodyPosition.X) < 0 ||
                    MotherBrainLateFightState.AtLeastBySignedDifference(_state.BodyPosition.X, 128);
                if (reachedForwardTarget)
                {
                    Phase3WalkCounter = MotherBrainWalkCredit.Of(128);
                    return;
                }

                _state.RequestWalkForward(Phase3TargetXPosition, forwardDelay);
                return;

            case MotherBrainPhase3WalkingPhase.RetreatQuickly:
                MotherBrainWalkResult quickRetreat =
                    _state.RequestWalkBackward(Phase3TargetXPosition, animationDelay: 2);
                if (!quickRetreat.ReachedTarget)
                    return;

                // Reaching the first target chooses another point fourteen pixels left but
                // does not fall through to the slow request. That request starts only on the
                // next standing AI call.
                Phase3TargetXPosition = _state.BodyPosition.X - 14;
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return;

            case MotherBrainPhase3WalkingPhase.RetreatSlowly:
                MotherBrainWalkResult slowRetreat =
                    _state.RequestWalkBackward(Phase3TargetXPosition, animationDelay: 4);
                if (!slowRetreat.ReachedTarget)
                    return;

                // The inch-forward setup writes all three values: forty walk-credit, the inch-forward
                // phase, and a one-pixel-ahead target used by later calls.
                SetPhase3WalkingToTryToInchForward(64);
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three walking phase {Phase3WalkingPhase}.");
        }
    }

    /// <summary>Resets walk credit and targets the next pixel ahead after a completed retreat.</summary>
    private void SetPhase3WalkingToTryToInchForward(int credit)
    {
        Phase3WalkCounter = MotherBrainWalkCredit.Of(credit);
        Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;
        Phase3TargetXPosition = _state.BodyPosition.X + 1;
    }

    /// <summary>Advances normal neck motion and the timed recovery after regular or hyper-beam recoil.</summary>
    private void StepPhase3Neck()
    {
        switch (Phase3NeckPhase)
        {
            case MotherBrainPhase3NeckPhase.Inactive:
                return;

            case MotherBrainPhase3NeckPhase.Normal:
                // The neck setup briefly writes lower index one and immediately overwrites it with
                // two before anything can observe it. Preserve the observable
                // result; the first write has no effect.
                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Bobbing;
                Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
                Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;

            case MotherBrainPhase3NeckPhase.SetupRecoilRecovery:
                Encounter.NeckMovementEnabled = true;
                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
                Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
                Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                Phase3NeckRecoilTimer = 16;

                // The neck step falls directly into its successor; the newly loaded sixteen is already
                // fifteen when this single update returns.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (Phase3NeckRecoilTimer != 0)
                {
                    Phase3NeckRecoilTimer--;
                    return;
                }

                // A decrement of zero branches negative without storing -1, so the readable
                // timer remains zero while the four-ring recovery animation plays.
                _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;

            case MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil:
                Encounter.NeckMovementEnabled = true;
                Phase3DisableAttacks = true;
                _state.SetHeadAnimation(MotherBrainHeadSegment.HyperBeamRecoil);
                Encounter.BrainMainShakeTimer = 50;
                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.HyperBeamRecoil;
                Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
                Encounter.UpperNeckMovement = MotherBrainNeckMovement.Raise;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                Phase3NeckRecoilTimer = 11;

                // Setup also falls through, making the externally visible first timer 10.
                goto case MotherBrainPhase3NeckPhase.HyperBeamRecoil;

            case MotherBrainPhase3NeckPhase.HyperBeamRecoil:
                if (Phase3NeckRecoilTimer != 0)
                {
                    Phase3NeckRecoilTimer--;
                    return;
                }

                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Bobbing;
                Phase3DisableAttacks = false;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupRecoilRecovery;
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three neck phase {Phase3NeckPhase}.");
        }
    }

    /// <summary>
    /// Runs the third phase recover make some distance body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseRecoverMakeSomeDistance()
    {
        // The recovery changes form before installing the setup wait. Its backward-walk
        // request uses target X-14 and delay index two; the ordinary helper may
        // legitimately reject it at the arena's hard left limit.
        Encounter.Form = MotherBrainForm.ThirdPhase;
        Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.RecoverSetupForFighting;
        Encounter.PhaseTimer.Count = 32;
        _state.RequestWalkBackward(
            CoordinateAxis.Wrap(_state.BodyPosition.X - 14),
            animationDelay: 2);
        return false;
    }

    /// <summary>
    /// Runs the third phase recover setup for fighting body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseRecoverSetupForFighting()
    {
        // The timer of 32 remains valid through zero and expires only when it wraps negative,
        // yielding 33 setup calls after the one-frame recovery producer above.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingMain;
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
            Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

            // The setup has no return after setting the three phases;
            // execution falls directly into the main fighting phase on this same enemy AI call.
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting main body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseFightingMain(RainbowBeamAttackFrame frame)
    {
        // The main fighting phase tests death before either the neck or the walking step. It installs the
        // death explosion/fade sequence without executing that phase's property
        // writes until the following body turn; no phase-two behavior leaks through.
        if (Encounter.Head.Health == 0)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.MoveToBackOfRoom;
            return false;
        }

        StepPhase3Neck();
        StepPhase3Walk(frame.RandomNumberSeed);

        // The walking animation changes pose later in the enemy animation stage. The
        // AI therefore still sees her standing pose on the call that requests a walk and may
        // also select an attack, since it reads the pose after the walk step runs.
        if (Encounter.Pose != MotherBrainBodyPose.Standing || Phase3DisableAttacks ||
            !frame.RandomNumberSeed.IsNegative)
            return false;

        if (frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.ThirdPhaseBombs))
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.BombPhase3);
        }
        else
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
        }

        Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingAttackCooldown;
        Encounter.PhaseTimer.Count = 64;
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting attack cooldown body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseFightingAttackCooldown()
    {
        // The attack cooldown returns to main only as its timer runs out. It
        // does not fall through, so the neck and walking steps pause for all 65 calls.
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingMain;
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainPhase3FightPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it within the same call.
    /// </summary>
    internal bool Run(RainbowBeamAttackFrame frame) => Encounter.Phase3FightPhase switch
    {
        MotherBrainPhase3FightPhase.RecoverMakeSomeDistance => RunThirdPhaseRecoverMakeSomeDistance(),
        MotherBrainPhase3FightPhase.RecoverSetupForFighting => RunThirdPhaseRecoverSetupForFighting(),
        MotherBrainPhase3FightPhase.FightingMain => RunThirdPhaseFightingMain(frame),
        MotherBrainPhase3FightPhase.FightingAttackCooldown => RunThirdPhaseFightingAttackCooldown(),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainPhase3FightPhase)} {Encounter.Phase3FightPhase}."),
    };
}
