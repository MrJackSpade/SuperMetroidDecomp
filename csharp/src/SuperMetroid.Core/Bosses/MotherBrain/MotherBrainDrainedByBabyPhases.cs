namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain drained by the Baby Metroid: taken aback, firing back, running out, retreating and turning gray.</summary>
internal sealed class MotherBrainDrainedByBabyPhases
{
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainDrainedByBabyPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>Current zero-based painful-walk stage, 0 through 8.</summary>
    [IntegerQuantity("zero-based count of painful-walk steps taken, 0 through 8")]
    public int PainfulWalkingStage { get; internal set; }

    /// <summary>Walk animation delay derived from the current stage: 2, 4, 6, 8 or 10 updates.</summary>
    public int PainfulWalkingAnimationDelay { get; internal set; }

    /// <summary>Pause timer used after each completed forward/backward body animation.</summary>
    public int PainfulWalkingPauseTimer { get; private set; }

    /// <summary>True while the nested painful walk is on its forward half.</summary>
    public bool PainfulWalkingForward { get; internal set; }

    /// <summary>Alternates painful forward and backward walk requests, advancing the stage only after its pause expires.</summary>
    internal void StepPainfulWalking()
    {
        // The game keeps four separate phases (request forward, wait forward,
        // request backward, wait backward). Two booleans express the same state without
        // hiding when a body animation is actually installed.
        if (PainfulWalkingPauseTimer != 0)
        {
            if (--PainfulWalkingPauseTimer == 0)
            {
                PainfulWalkingStage++;
                PainfulWalkingForward = !PainfulWalkingForward;
            }
            return;
        }

        MotherBrainWalkResult walk = PainfulWalkingForward
            ? _state.RequestWalkForward(72, PainfulWalkingAnimationDelay)
            : _state.RequestWalkBackward(40, PainfulWalkingAnimationDelay);
        if (walk.ReachedTarget)
        {
            int timerIndex = Math.Min(PainfulWalkingStage, 7);
            PainfulWalkingPauseTimer = MotherBrainPainfulWalking.PauseTimer(timerIndex);
        }
    }

    /// <summary>
    /// Runs the second phase drained by baby taken aback body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyTakenAback()
    {
        // The taken-aback step is not executed by the Baby. It runs on Mother Brain's next
        // actor turn, changes form/neck geometry, seeds 48, and falls through so
        // that this first regain-balance call immediately decrements it to 47.
        Encounter.Form = MotherBrainForm.DrainedByBabyMetroid;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.Raise;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.TakenAback;
        Encounter.DrainedPhase = MotherBrainDrainedPhase.RegainBalance;
        Encounter.PhaseTimer.Count = 48;
        return true;
    }

    /// <summary>
    /// Runs the second phase drained by baby regain balance body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyRegainBalance(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.RainbowColours = true;
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            PainfulWalkingForward = true;
            PainfulWalkingStage = 0;
            PainfulWalkingAnimationDelay = 2;
            PainfulWalkingPauseTimer = 0;
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
            Encounter.DrainedPhase = MotherBrainDrainedPhase.FiringRainbowBeam;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby firing rainbow beam body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyFiringRainbowBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        // The rainbow step refreshes the shake timer only if another subsystem has cleared it.
        // The head animation owns any decrement; retaining the write gate avoids
        // fabricating a body-side countdown that does not exist in this step.
        if (Encounter.BrainMainShakeTimer == 0)
            Encounter.BrainMainShakeTimer = 50;
        requests.RainbowColours = true;
        StepPainfulWalking();

        // The outer phase updates delay and neck delta after every step of the painful walk, so the sixth step is seen
        // on the update that ends the pause before it, before its forward walk has begun.
        if (PainfulWalkingStage < MotherBrainPainfulWalkingDefinitions.StageCount)
        {
            PainfulWalkingAnimationDelay =
                MotherBrainPainfulWalking.AnimationDelay(PainfulWalkingStage);
            Encounter.NeckTurnPerUpdate = MotherBrainPainfulWalking.NeckTurn(PainfulWalkingStage);
        }
        if (PainfulWalkingStage == 6)
        {
            _state.RainbowBeamSoundPlaying = false;
            Encounter.DrainedPhase = MotherBrainDrainedPhase.RainbowBeamRunOut;
            requests.DrainedBeamRanOut = true;
            requests.SoundQueued = true; // The rainbow-drain sound stops as the beam runs out.
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby rainbow beam run out body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyRainbowBeamRunOut()
    {
        StepPainfulWalking();
        if (PainfulWalkingStage >= 8)
        {
            Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.Raise;
            _state.SetHeadAnimation(MotherBrainHeadSegment.DyingDrool);
            Encounter.DrainedPhase = MotherBrainDrainedPhase.MoveToBackOfRoom;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby move to back of room body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyMoveToBackOfRoom()
    {
        // The walk back is the really-slow backward walk, the painful walk's last animation.
        MotherBrainWalkResult moveToBack = _state.RequestWalkBackwardReallySlow(targetX: 40);
        if (moveToBack.ReachedTarget)
        {
            Encounter.DrainedPhase = MotherBrainDrainedPhase.GoIntoLowPowerMode;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.None;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby go into low power mode body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyGoIntoLowPowerMode()
    {
        // The brain enemy advances the one-way neck raises on its own. The body
        // clears drool once both indices reach zero, before waiting for its pose.
        if (Encounter.LowerNeckMovement == MotherBrainNeckMovement.None &&
            Encounter.UpperNeckMovement == MotherBrainNeckMovement.None)
        {
            Encounter.DroolGenerationEnabled = false;
            if (Encounter.Pose != MotherBrainBodyPose.Standing)
                return false;
            _state.SetBodyAnimation(MotherBrainBodySegment.CrouchFast);
            Encounter.DrainedPhase = MotherBrainDrainedPhase.PrepareTransitionToGrey;
            Encounter.PhaseTimer.Count = 64;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby prepare transition to grey body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyPrepareTransitionToGrey()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DrainedGreying.Restart();
            Encounter.DrainedPhase = MotherBrainDrainedPhase.TransitionToGrey;
            Encounter.PhaseTimer.Count = 16;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby transition to grey body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyTransitionToGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.PhaseTimer.Count = 16;

            // The body greys over eight steps, one each time the timer runs out; the ninth finishes the transition
            // without new colours.
            int greyStep = Encounter.DrainedGreying.TakeStep();
            if (greyStep < 8)
                requests.DrainColourStep = greyStep;
            if (greyStep >= 8)
            {
                Encounter.Head.Health = 36000;
                _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Corpse;
                Encounter.SmallPurpleBreathGenerationEnabled = false;
                Encounter.Form = MotherBrainForm.SecondPhase;
                Encounter.RevivalPhase = MotherBrainRevivalPhase.InanimateGrey;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainDrainedPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
    /// </summary>
    internal bool Run(MotherBrainRainbowBeamStepRequests requests) => Encounter.DrainedPhase switch
    {
        MotherBrainDrainedPhase.TakenAback => RunSecondPhaseDrainedByBabyTakenAback(),
        MotherBrainDrainedPhase.RegainBalance => RunSecondPhaseDrainedByBabyRegainBalance(requests),
        MotherBrainDrainedPhase.FiringRainbowBeam => RunSecondPhaseDrainedByBabyFiringRainbowBeam(requests),
        MotherBrainDrainedPhase.RainbowBeamRunOut => RunSecondPhaseDrainedByBabyRainbowBeamRunOut(),
        MotherBrainDrainedPhase.MoveToBackOfRoom => RunSecondPhaseDrainedByBabyMoveToBackOfRoom(),
        MotherBrainDrainedPhase.GoIntoLowPowerMode => RunSecondPhaseDrainedByBabyGoIntoLowPowerMode(),
        MotherBrainDrainedPhase.PrepareTransitionToGrey => RunSecondPhaseDrainedByBabyPrepareTransitionToGrey(),
        MotherBrainDrainedPhase.TransitionToGrey => RunSecondPhaseDrainedByBabyTransitionToGrey(requests),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainDrainedPhase)} {Encounter.DrainedPhase}."),
    };
}
