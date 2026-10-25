namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's third phase: recovering, setting up and fighting Samus after the baby cutscene.</summary>
internal sealed class MotherBrainThirdPhaseFight
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainThirdPhaseFight(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>
    /// Phase-three walking function selected through native long word <c>$7E:801E</c>.
    /// This is deliberately separate from the visible body instruction list: the function
    /// decides what Mother Brain wants to do only while the bytecode-owned pose is standing.
    /// </summary>
    public MotherBrainPhase3WalkingPhase Phase3WalkingPhase { get; private set; } =
        MotherBrainPhase3WalkingPhase.Inactive;

    /// <summary>
    /// The walk-credit word <c>$7E:780E</c>, which an encounter shares with the sequence it attaches; a standalone
    /// sequence keeps its own.
    /// </summary>
    private MotherBrainWalkCounter _walk = new();

    /// <summary>
    /// Mother Brain's walk credit (<c>$7E:780E</c>) as the sequence uses it. Ordinary projectiles subtract
    /// <c>$0100</c>; Hyper Beam subtracts <c>$010A</c>; the walking function adds
    /// <c>$0020</c> whenever it gets a standing AI call.
    /// </summary>
    public MotherBrainWalkCredit Phase3WalkCounter
    {
        get => _walk.Credit;
        internal set => _walk.Credit = value;
    }

    /// <summary>Current phase-three walking target X in room pixels (<c>$7E:803A</c>), within a pixel of the body's walking range.</summary>
    public int Phase3TargetXPosition { get; private set; }

    /// <summary>Phase-three neck function selected through native long word <c>$7E:801A</c>.</summary>
    public MotherBrainPhase3NeckPhase Phase3NeckPhase { get; internal set; } =
        MotherBrainPhase3NeckPhase.Inactive;

    /// <summary>Signed-underflow timer shared by the two phase-three recoil functions.</summary>
    public WrappingCountdown Phase3NeckFunctionTimer { get; private set; }

    /// <summary>
    /// Whether phase-three attacks are suspended (<c>$7E:803E</c>). Hyper Beam recoil sets it
    /// and the recoil timer clears it before the recovery pose begins.
    /// </summary>
    public bool Phase3DisableAttacks { get; private set; }

    /// <summary>Schedules phase-three inching and two retreat speeds only while the body is standing.</summary>
    private bool StepPhase3WalkingHandler(MotherBrainRandomSample randomNumberSeed)
    {
        // `$C25A` refuses to call the walking function while body bytecode advertises any
        // nonzero pose. This makes each request edge-triggered: the requested walk runs to
        // its standing opcode before the scheduler is allowed to inspect its target again.
        if (_state.Body.Pose != MotherBrainBodyPose.Standing)
            return false;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return false;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter.IsEmpty)
                {
                    // Zero credit does not merely pause. `$C2A1` chooses a point fourteen
                    // pixels left, installs the quick-retreat function, and falls into it.
                    Phase3TargetXPosition = _state.Body.Position.X - 14;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // Native ADC/CMP is unsigned here. A wrapped counter can therefore spend
                // more calls below `$0100`; no host integer widening or saturation belongs.
                Phase3WalkCounter = Phase3WalkCounter.AddCredit(32);
                if (Phase3WalkCounter.BelowStep)
                    return false;

                Phase3TargetXPosition = _state.Body.Position.X + 1;
                int forwardDelay = (randomNumberSeed.HasBit(1) ? 2 : 0) + 4;

                // MakeMotherBrainWalkForwards reports carry only for a strictly overshot
                // target or the independent X `$80` limit. Equality still requests a walk.
                bool reachedForwardTarget =
                    unchecked((short)(Phase3TargetXPosition - _state.Body.Position.X)) < 0 ||
                    MotherBrainLateFightState.AtLeastBySignedDifference(_state.Body.Position.X, 128);
                if (reachedForwardTarget)
                {
                    Phase3WalkCounter = MotherBrainWalkCredit.Of(128);
                    return false;
                }

                return _state.RequestWalkForward(Phase3TargetXPosition, forwardDelay).Requested;

            case MotherBrainPhase3WalkingPhase.RetreatQuickly:
                MotherBrainWalkResult quickRetreat =
                    _state.RequestWalkBackward(Phase3TargetXPosition, animationDelay: 2);
                if (!quickRetreat.ReachedTarget)
                    return quickRetreat.Requested;

                // Reaching the first target chooses another point fourteen pixels left but
                // does not fall through to the slow request. That request starts only on the
                // next standing AI call.
                Phase3TargetXPosition = _state.Body.Position.X - 14;
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return false;

            case MotherBrainPhase3WalkingPhase.RetreatSlowly:
                MotherBrainWalkResult slowRetreat =
                    _state.RequestWalkBackward(Phase3TargetXPosition, animationDelay: 4);
                if (!slowRetreat.ReachedTarget)
                    return slowRetreat.Requested;

                // `$C313` writes all three words: forty walk-credit, the inch-forward
                // function, and a one-pixel-ahead target used by later calls.
                SetPhase3WalkingToTryToInchForward(64);
                return false;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three walking function {Phase3WalkingPhase}.");
        }
    }

    /// <summary>Resets walk credit and targets the next pixel ahead after a completed retreat.</summary>
    private void SetPhase3WalkingToTryToInchForward(int credit)
    {
        Phase3WalkCounter = MotherBrainWalkCredit.Of(credit);
        Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;
        Phase3TargetXPosition = _state.Body.Position.X + 1;
    }

    /// <summary>Advances normal neck motion and the timed recovery after regular or hyper-beam recoil.</summary>
    private void StepPhase3NeckHandler()
    {
        switch (Phase3NeckPhase)
        {
            case MotherBrainPhase3NeckPhase.Inactive:
                return;

            case MotherBrainPhase3NeckPhase.Normal:
                // `$C330` briefly writes lower index one and immediately overwrites it with
                // two before any other code can observe the word. Preserve the observable
                // result while documenting the retail no-op (`>_<` in the disassembly).
                _state.NeckAngleDelta = 128;
                _state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
                _state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;

            case MotherBrainPhase3NeckPhase.SetupRecoilRecovery:
                _state.NeckMovementEnabled = true;
                _state.NeckAngleDelta = 1280;
                _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
                _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                Phase3NeckFunctionTimer = WrappingCountdown.Start(16);

                // `$C354` falls directly into `$C37B`; the newly loaded sixteen is already
                // fifteen when this single handler call returns.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (!Phase3NeckFunctionTimer.IsZero)
                {
                    Phase3NeckFunctionTimer = Phase3NeckFunctionTimer.Decrement().Remaining;
                    return;
                }

                // DEC of zero branches negative without storing `$FFFF`, so the readable
                // timer remains zero while the four-ring recovery list is installed.
                _state.SetHeadProgram(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;

            case MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil:
                _state.NeckMovementEnabled = true;
                Phase3DisableAttacks = true;
                _state.SetHeadProgram(MotherBrainHeadProgram.HyperBeamRecoil);
                _state.BrainMainShakeTimer = WrappingCountdown.Start(50);
                _state.NeckAngleDelta = 2304;
                _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
                _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Raise;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                Phase3NeckFunctionTimer = WrappingCountdown.Start(11);

                // Setup also falls through, making the externally visible first timer `$0A`.
                goto case MotherBrainPhase3NeckPhase.HyperBeamRecoil;

            case MotherBrainPhase3NeckPhase.HyperBeamRecoil:
                if (!Phase3NeckFunctionTimer.IsZero)
                {
                    Phase3NeckFunctionTimer = Phase3NeckFunctionTimer.Decrement().Remaining;
                    return;
                }

                _state.NeckAngleDelta = 128;
                Phase3DisableAttacks = false;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupRecoilRecovery;
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three neck function {Phase3NeckPhase}.");
        }
    }

    /// <summary>
    /// Runs the third phase recover make some distance body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseRecoverMakeSomeDistance(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$C1CF` changes form before installing the setup wait. Its backward-walk
        // request uses target X-14 and delay index two; the ordinary helper may
        // legitimately reject it at the arena's hard left limit.
        _state.Body.Form = MotherBrainForm.ThirdPhase;
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting;
        _state.FunctionTimer = WrappingCountdown.Start(32);
        requests.BodyWalkRequested = _state.RequestWalkBackward(
            unchecked((ushort)(_state.Body.Position.X - 14)),
            animationDelay: 2).Requested;
        return false;
    }

    /// <summary>
    /// Runs the third phase recover setup for fighting body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseRecoverSetupForFighting()
    {
        // `$20` remains valid through zero and expires only when DEC wraps negative,
        // yielding 33 setup calls after the one-frame `$C1CF` producer above.
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseFightingMain;
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
            Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

            // `$C1F0-$C205` has no RTS after installing the three function pointers;
            // execution falls directly into `$C209` on this same enemy AI call.
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting main body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseFightingMain(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // `$C209` tests death before either sub-handler. It installs the translated
        // `$AEE1` explosion/fade sequence without executing that function's property
        // writes until the following body turn; no phase-two behavior leaks through.
        if (_state.BrainHealth == 0)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom;
            return false;
        }

        StepPhase3NeckHandler();
        requests.BodyWalkRequested = StepPhase3WalkingHandler(frame.RandomNumberSeed);

        // Walking bytecode changes pose later in the enemy instruction stage. The
        // AI therefore still sees pose zero on the call that requests a walk and may
        // also select an attack, exactly as the source's post-handler `$C21B` load.
        if (_state.Body.Pose != MotherBrainBodyPose.Standing || Phase3DisableAttacks ||
            !frame.RandomNumberSeed.IsNegative)
            return false;

        if (frame.RandomNumberSeed.LowByte < 128)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.BombPhase3);
        }
        else
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
        }

        _state.Phase = MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown;
        _state.FunctionTimer = WrappingCountdown.Start(64);
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting attack cooldown body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseFightingAttackCooldown()
    {
        // `$C24E` accepts timer zero and returns to main only after underflow. It
        // does not fall through, so neck and walking handlers pause for all 65 calls.
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseFightingMain;
        return false;
    }

    /// <summary>Shares the walk counter the room's Mother Brain state keeps with the third-phase walk.</summary>
    internal void ShareWalkCounter(MotherBrainWalkCounter walk) => _walk = walk;
}
