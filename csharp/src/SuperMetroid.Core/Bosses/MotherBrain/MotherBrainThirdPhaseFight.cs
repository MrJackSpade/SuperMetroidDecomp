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
    /// 256; Hyper Beam subtracts 266; the walking adds 32 on each update she spends standing.
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
        // The walking waits while her body shows any pose but standing, so each walk she asks for runs until it
        // stands her again before the walking looks at its target again.
        if (Encounter.Pose != MotherBrainBodyPose.Standing)
            return;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter.IsEmpty)
                {
                    // With no credit left she retreats: she picks a point fourteen pixels left and starts retreating
                    // quickly in this same update.
                    Phase3TargetXPosition = _state.BodyPosition.X - 14;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // Past the count's top the credit starts again from zero, and spends more updates below a whole
                // step.
                Phase3WalkCounter = Phase3WalkCounter.AddCredit(32);
                if (Phase3WalkCounter.BelowStep)
                    return;

                Phase3TargetXPosition = _state.BodyPosition.X + 1;
                int forwardDelay = (randomNumberSeed.IsSet(MotherBrainRandomDefinitions.ForwardInchLongerDelay) ? 2 : 0) + 4;

                // The forward walk reports arrival for an overshot target or at its X 128 limit. The target lies one
                // pixel ahead of her, which she has not overshot, so only the limit reports it here.
                bool reachedForwardTarget = _state.BodyPosition.X >= 128;
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

                // Reaching the first target picks another point fourteen pixels left; the slow retreat to it starts
                // on the next update she spends standing.
                Phase3TargetXPosition = _state.BodyPosition.X - 14;
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return;

            case MotherBrainPhase3WalkingPhase.RetreatSlowly:
                MotherBrainWalkResult slowRetreat =
                    _state.RequestWalkBackward(Phase3TargetXPosition, animationDelay: 4);
                if (!slowRetreat.ReachedTarget)
                    return;

                // She goes back to inching forward with a credit of 64 toward a point one pixel ahead.
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
                // The neck goes back to its ordinary bob.
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

                // The recovery takes its first step in this same update, so its timer of sixteen already reads
                // fifteen when the update ends.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (Phase3NeckRecoilTimer != 0)
                {
                    Phase3NeckRecoilTimer--;
                    return;
                }

                // The timer stays at zero once it runs out, while the four-ring recovery animation plays.
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

                // The recoil takes its first step in this same update, so its timer of 11 first reads 10.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseRecoverMakeSomeDistance()
    {
        // The recovery changes her form before the wait to fight, and she walks back fourteen pixels quickly; at
        // the arena's left limit she stays where she is.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseRecoverSetupForFighting()
    {
        // The timer of 32 runs out on its 33rd step, so the wait takes 33 updates after the one-update recovery.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingMain;
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
            Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

            // The fight itself starts in this same update.
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting main body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseFightingMain(RainbowBeamAttackFrame frame)
    {
        // The fight checks for her death before the neck or the walking steps. Her death starts on the next update;
        // nothing of the fight runs in between.
        if (Encounter.Head.Health == 0)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.MoveToBackOfRoom;
            return false;
        }

        StepPhase3Neck();
        StepPhase3Walk(frame.RandomNumberSeed);

        // Her walking animation changes her pose later in the update, so on the update she asks to walk she still
        // stands and may also choose an attack.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseFightingAttackCooldown()
    {
        // The attack's cooldown ends the update its timer runs out, and the fight resumes on the next, so the neck
        // and the walking pause for all 65 updates.
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingMain;
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainPhase3FightPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
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
