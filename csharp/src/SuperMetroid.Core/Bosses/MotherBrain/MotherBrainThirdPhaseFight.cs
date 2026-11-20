namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's third phase: recovering, setting up and fighting Samus after the baby cutscene.</summary>
internal sealed class MotherBrainThirdPhaseFight
{
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
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
    /// Apart from the visible body animation, the walking decides what Mother Brain wants to do only while her animated
    /// pose is standing.
    /// </summary>
    public MotherBrainPhase3WalkingPhase Phase3WalkingPhase { get; private set; } =
        MotherBrainPhase3WalkingPhase.Inactive;

    /// <summary>The walk credit of the encounter the sequence is attached to, which its walking and shot reactions also read and write.</summary>
    private readonly MotherBrainWalkCounter _walk;

    /// <summary>
    /// Mother Brain's walk credit as the sequence uses it. Ordinary projectiles and the Hyper Beam each
    /// subtract their cost, and the walking adds credit on each update she spends standing (<see cref="MotherBrainWalkCredit"/>).
    /// </summary>
    public MotherBrainWalkCredit Phase3WalkCounter
    {
        get => _walk.Credit;
        internal set => _walk.Credit = value;
    }

    /// <summary>Current phase-three walking target X in room pixels, within a pixel of the body's walking range.</summary>
    public int Phase3TargetXPosition { get; private set; }

    /// <summary>Mother Brain's phase-three neck movement.</summary>
    internal MotherBrainPhase3NeckPhase Phase3NeckPhase { private get; set; } =
        MotherBrainPhase3NeckPhase.Inactive;

    /// <summary>The wait of her neck's recoil from a Hyper Beam hit (#2469), which its recoil phase runs out.</summary>
    public BossCountdown HyperBeamRecoilWait { get; } = new();

    /// <summary>The wait of her neck's recovery after a recoil (#2469), which its recovery phase runs out.</summary>
    public BossCountdown RecoilRecoveryWait { get; } = new();

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

        // Each case ends the update or goes on into the case it has just entered (#2553).
        while (true)
        {
            if (Phase3WalkingPhase == MotherBrainPhase3WalkingPhase.Inactive)
            {
                return;
            }
            else if (Phase3WalkingPhase == MotherBrainPhase3WalkingPhase.TryToInchForward)
            {
                if (Phase3WalkCounter.IsEmpty)
                {
                    // With no credit left she retreats: she picks a point her retreat's distance left and starts retreating
                    // quickly in this same update.
                    Phase3TargetXPosition = _state.BodyPosition.X - MotherBrainLateFightDefinitions.Phase3RetreatPixels;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    continue;
                }

                // Past the count's top the credit starts again from zero, and spends more updates below a whole
                // step.
                Phase3WalkCounter = Phase3WalkCounter.AddCredit(MotherBrainLateFightDefinitions.Phase3CreditPerTurn);
                if (Phase3WalkCounter.BelowStep)
                    return;

                Phase3TargetXPosition = _state.BodyPosition.X + 1;
                MotherBrainWalkSpeed forwardSpeed = randomNumberSeed.Takes(MotherBrainRandomDefinitions.ForwardInchWaitsLonger)
                    ? MotherBrainWalkSpeed.Medium
                    : MotherBrainWalkSpeed.Fast;

                // The forward walk reports arrival for an overshot target or at the arena's front limit. The target lies one
                // pixel ahead of her, which she has not overshot, so only the limit reports it here.
                bool reachedForwardTarget = _state.BodyPosition.X >= MotherBrainFightDefinitions.FrontLimitX;
                if (reachedForwardTarget)
                {
                    Phase3WalkCounter = MotherBrainWalkCredit.Of(MotherBrainFightDefinitions.AfterStepCredit);
                    return;
                }

                _state.RequestWalkForward(Phase3TargetXPosition, forwardSpeed);
                return;
            }
            else if (Phase3WalkingPhase == MotherBrainPhase3WalkingPhase.RetreatQuickly)
            {
                if (!_state.RequestWalkBackward(Phase3TargetXPosition, MotherBrainWalkSpeed.ReallyFast))
                    return;

                // Reaching the first target picks another point a retreat's step left; the slow retreat to it starts
                // on the next update she spends standing.
                Phase3TargetXPosition = _state.BodyPosition.X - MotherBrainLateFightDefinitions.Phase3RetreatPixels;
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return;
            }
            else
            {
                // The remaining member, MotherBrainPhase3WalkingPhase.RetreatSlowly.
                if (!_state.RequestWalkBackward(Phase3TargetXPosition, MotherBrainWalkSpeed.Fast))
                    return;

                // She goes back to inching forward with her after-retreat credit toward a point one pixel ahead.
                SetPhase3WalkingToTryToInchForward(MotherBrainLateFightDefinitions.Phase3AfterRetreatCredit);
                return;
            }
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
        // Each case ends the update or goes on into the case it has just entered (#2553).
        while (true)
        {
            if (Phase3NeckPhase == MotherBrainPhase3NeckPhase.Inactive)
            {
                return;
            }
            else if (Phase3NeckPhase == MotherBrainPhase3NeckPhase.Normal)
            {
                // The neck goes back to its ordinary bob.
                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Bobbing;
                Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
                Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;
            }
            else if (Phase3NeckPhase == MotherBrainPhase3NeckPhase.StartRecoilRecovery)
            {
                Encounter.NeckMovementEnabled = true;
                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
                Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
                Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                RecoilRecoveryWait.LastFor(MotherBrainLateFightDefinitions.RecoilRecoveryUpdates);

                // The recovery's first update is this same update.
                continue;
            }
            else if (Phase3NeckPhase == MotherBrainPhase3NeckPhase.RecoilRecovery)
            {
                if (RecoilRecoveryWait.StepKeepsRunning())
                    return;

                // On the recovery's last update the four-ring recovery animation starts.
                _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;
            }
            else if (Phase3NeckPhase == MotherBrainPhase3NeckPhase.StartHyperBeamRecoil)
            {
                Encounter.NeckMovementEnabled = true;
                Phase3DisableAttacks = true;
                _state.SetHeadAnimation(MotherBrainHeadSegment.HyperBeamRecoil);
                Encounter.BrainShake.ShakeFor(MotherBrainFightDefinitions.BrainShakeUpdates);
                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.HyperBeamRecoil;
                Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
                Encounter.UpperNeckMovement = MotherBrainNeckMovement.Raise;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                HyperBeamRecoilWait.LastFor(MotherBrainLateFightDefinitions.HyperBeamRecoilUpdates);

                // The recoil's first update is this same update.
                continue;
            }
            else
            {
                // The remaining member, MotherBrainPhase3NeckPhase.HyperBeamRecoil.
                if (HyperBeamRecoilWait.StepKeepsRunning())
                    return;

                Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Bobbing;
                Phase3DisableAttacks = false;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.StartRecoilRecovery;
                return;
            }
        }
    }

    /// <summary>
    /// Changes her to her third phase's form, walks her back quickly and starts the wait before she fights. True when
    /// her next phase runs on in this same update.
    /// </summary>
    internal bool RecoverAndBackAway()
    {
        // The recovery changes her form before the wait to fight, and she walks back her retreat's distance quickly; at
        // the arena's left limit she stays where she is.
        Encounter.Form = MotherBrainForm.ThirdPhase;
        _state.Phase3FightPhase = MotherBrainPhase3FightPhase.RecoverSetupForFighting;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.RecoverSetupUpdates);
        _state.RequestWalkBackward(
            _state.BodyPosition.X - MotherBrainLateFightDefinitions.Phase3RetreatPixels,
            MotherBrainWalkSpeed.ReallyFast);
        return false;
    }

    /// <summary>
    /// Waits out the one-update recovery, then starts the fight with her neck and walking at their start. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool WaitToFight()
    {
        // The wait to fight starts on the update after the one-update recovery.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingMain;
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
            Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

            // The fight itself starts in this same update.
            return true;
        }
        return false;
    }

    /// <summary>
    /// Fights: moves her neck, walks and chooses whether to attack, until her health runs out. True when her next phase
    /// runs on in this same update.
    /// </summary>
    internal bool Fight(RainbowBeamAttackFrame frame)
    {
        // The fight checks for her death before the neck or the walking steps. Her death starts on the next update;
        // nothing of the fight runs in between.
        if (Encounter.Head.Health == 0)
        {
            _state.DeathPhase = MotherBrainDeathPhase.MoveToBackOfRoom;
            return false;
        }

        StepPhase3Neck();
        StepPhase3Walk(frame.RandomNumberSeed);

        // Her walking animation changes her pose later in the update, so on the update she asks to walk she still
        // stands and may also choose an attack.
        if (Encounter.Pose != MotherBrainBodyPose.Standing || Phase3DisableAttacks ||
            !frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.ThirdPhaseAttacks))
            return false;

        if (frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.ThirdPhaseBombs))
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.BombPhase3);
        }
        else
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
        }

        _state.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingAttackCooldown;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.Phase3AttackCooldownUpdates);
        return false;
    }

    /// <summary>
    /// Waits out an attack's cooldown, then fights again. True when her next phase runs on in this same update.
    /// </summary>
    internal bool CoolDownFromAttack()
    {
        // The attack's cooldown ends on its last update and the fight resumes on the next, so the neck and the walking
        // pause for every update of the cooldown.
        if (Encounter.PhaseTimer.StepRunsOut())
            _state.Phase3FightPhase = MotherBrainPhase3FightPhase.FightingMain;
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainPhase3FightPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
    /// </summary>
    internal bool Run(RainbowBeamAttackFrame frame) => _state.Phase3FightPhase switch
    {
        MotherBrainPhase3FightPhase.RecoverMakeSomeDistance => RecoverAndBackAway(),
        MotherBrainPhase3FightPhase.RecoverSetupForFighting => WaitToFight(),
        MotherBrainPhase3FightPhase.FightingMain => Fight(frame),
        MotherBrainPhase3FightPhase.FightingAttackCooldown => CoolDownFromAttack(),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainPhase3FightPhase)} {_state.Phase3FightPhase}."),
    };
}
