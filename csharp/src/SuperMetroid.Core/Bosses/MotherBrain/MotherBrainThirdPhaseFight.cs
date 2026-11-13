namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's third phase: recovering, setting up and fighting Samus after the baby cutscene.</summary>
internal sealed class MotherBrainThirdPhaseFight
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs and the brain's flags, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainThirdPhaseFight(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>
    /// The phase-three walking function.
    /// This is deliberately separate from the visible body program: the function
    /// decides what Mother Brain wants to do only while the program-owned pose is standing.
    /// </summary>
    public MotherBrainPhase3WalkingPhase Phase3WalkingPhase { get; private set; } =
        MotherBrainPhase3WalkingPhase.Inactive;

    /// <summary>
    /// The walk credit, which an encounter shares with the sequence it attaches; a standalone
    /// sequence keeps its own.
    /// </summary>
    private MotherBrainWalkCounter _walk = new();

    /// <summary>
    /// Mother Brain's walk credit as the sequence uses it. Ordinary projectiles subtract
    /// 256; Hyper Beam subtracts 266; the walking function adds
    /// 32 whenever it gets a standing AI call.
    /// </summary>
    public MotherBrainWalkCredit Phase3WalkCounter
    {
        get => _walk.Credit;
        internal set => _walk.Credit = value;
    }

    /// <summary>Current phase-three walking target X in room pixels, within a pixel of the body's walking range.</summary>
    public int Phase3TargetXPosition { get; private set; }

    /// <summary>The phase-three neck function.</summary>
    public MotherBrainPhase3NeckPhase Phase3NeckPhase { get; internal set; } =
        MotherBrainPhase3NeckPhase.Inactive;

    /// <summary>Countdown shared by the two phase-three recoil functions; it stops at zero.</summary>
    [IntegerQuantity("frames left in the recoil; the death sequence waits for the tenth")]
    public int Phase3NeckFunctionTimer { get; private set; }

    /// <summary>
    /// Whether phase-three attacks are suspended. Hyper Beam recoil sets it
    /// and the recoil timer clears it before the recovery pose begins.
    /// </summary>
    public bool Phase3DisableAttacks { get; private set; }

    /// <summary>Schedules phase-three inching and two retreat speeds only while the body is standing.</summary>
    private bool StepPhase3WalkingHandler(MotherBrainRandomSample randomNumberSeed)
    {
        // The phase-three scheduler refuses to call the walking function while body bytecode advertises any
        // nonzero pose. This makes each request edge-triggered: the requested walk runs to
        // its standing cue before the scheduler is allowed to inspect its target again.
        if (_state.Body.Pose != MotherBrainBodyPose.Standing)
            return false;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return false;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter.IsEmpty)
                {
                    // Zero credit does not merely pause. The retreat chooses a point fourteen
                    // pixels left, installs the quick-retreat function, and falls into it.
                    Phase3TargetXPosition = _state.Body.Position.X - 14;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // The addition and comparison are unsigned here. A wrapped counter can therefore spend
                // more calls below 256; no host integer widening or saturation belongs.
                Phase3WalkCounter = Phase3WalkCounter.AddCredit(32);
                if (Phase3WalkCounter.BelowStep)
                    return false;

                Phase3TargetXPosition = _state.Body.Position.X + 1;
                int forwardDelay = (randomNumberSeed.IsSet(MotherBrainRandomDefinitions.ForwardInchLongerDelay) ? 2 : 0) + 4;

                // The forward walk reports arrival only for a strictly overshot
                // target or the independent X 128 limit. Equality still requests a walk.
                bool reachedForwardTarget =
                    CoordinateAxis.Difference(Phase3TargetXPosition, _state.Body.Position.X) < 0 ||
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

                // The inch-forward setup writes all three values: forty walk-credit, the inch-forward
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
                // The neck setup briefly writes lower index one and immediately overwrites it with
                // two before any other code can observe the word. Preserve the observable
                // result; the first write has no effect.
                _state.NeckAngleDelta = 128;
                _state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
                _state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;

            case MotherBrainPhase3NeckPhase.SetupRecoilRecovery:
                _state.NeckMovementEnabled = true;
                _state.NeckAngleDelta = 1280;
                _state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
                _state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                Phase3NeckFunctionTimer = 16;

                // The neck step falls directly into its successor; the newly loaded sixteen is already
                // fifteen when this single handler call returns.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (Phase3NeckFunctionTimer != 0)
                {
                    Phase3NeckFunctionTimer--;
                    return;
                }

                // A decrement of zero branches negative without storing -1, so the readable
                // timer remains zero while the four-ring recovery list is installed.
                _state.SetHeadProgram(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;

            case MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil:
                _state.NeckMovementEnabled = true;
                Phase3DisableAttacks = true;
                _state.SetHeadProgram(MotherBrainHeadProgram.HyperBeamRecoil);
                _state.BrainMainShakeTimer = 50;
                _state.NeckAngleDelta = 2304;
                _state.LowerNeckMovement = MotherBrainNeckMovement.Raise;
                _state.UpperNeckMovement = MotherBrainNeckMovement.Raise;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                Phase3NeckFunctionTimer = 11;

                // Setup also falls through, making the externally visible first timer 10.
                goto case MotherBrainPhase3NeckPhase.HyperBeamRecoil;

            case MotherBrainPhase3NeckPhase.HyperBeamRecoil:
                if (Phase3NeckFunctionTimer != 0)
                {
                    Phase3NeckFunctionTimer--;
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
    /// Runs the third phase recover make some distance body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseRecoverMakeSomeDistance(MotherBrainRainbowBeamStepRequests requests)
    {
        // The recovery changes form before installing the setup wait. Its backward-walk
        // request uses target X-14 and delay index two; the ordinary helper may
        // legitimately reject it at the arena's hard left limit.
        _state.Body.Form = MotherBrainForm.ThirdPhase;
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting;
        _state.FunctionTimer = 32;
        requests.BodyWalkRequested = _state.RequestWalkBackward(
            CoordinateAxis.Wrap(_state.Body.Position.X - 14),
            animationDelay: 2).Requested;
        return false;
    }

    /// <summary>
    /// Runs the third phase recover setup for fighting body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseRecoverSetupForFighting()
    {
        // The timer of 32 remains valid through zero and expires only when it wraps negative,
        // yielding 33 setup calls after the one-frame recovery producer above.
        if (--_state.FunctionTimer < 0)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseFightingMain;
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
            Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

            // The setup has no return after installing the three functions;
            // execution falls directly into the main fighting function on this same enemy AI call.
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting main body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseFightingMain(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The main fighting function tests death before either sub-handler. It installs the translated
        // death explosion/fade sequence without executing that function's property
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
        // also select an attack, exactly as the source's post-handler pose load.
        if (_state.Body.Pose != MotherBrainBodyPose.Standing || Phase3DisableAttacks ||
            !frame.RandomNumberSeed.IsNegative)
            return false;

        if (frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.ThirdPhaseBombs))
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.BombPhase3);
        }
        else
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
        }

        _state.Phase = MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown;
        _state.FunctionTimer = 64;
        return false;
    }

    /// <summary>
    /// Runs the third phase fighting attack cooldown body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseFightingAttackCooldown()
    {
        // The attack cooldown accepts timer zero and returns to main only after underflow. It
        // does not fall through, so neck and walking handlers pause for all 65 calls.
        if (--_state.FunctionTimer < 0)
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseFightingMain;
        return false;
    }

    /// <summary>Shares the walk counter the room's Mother Brain state keeps with the third-phase walk.</summary>
    internal void ShareWalkCounter(MotherBrainWalkCounter walk) => _walk = walk;
}
