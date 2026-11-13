namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain drained by the Baby Metroid: taken aback, firing back, running out, retreating and turning gray.</summary>
internal sealed class MotherBrainDrainedByBabyPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs and the brain's flags, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

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

    /// <summary>Pause timer used after each completed forward/backward body list.</summary>
    public int PainfulWalkingFunctionTimer { get; private set; }

    /// <summary>True while the nested painful-walk function is on its forward half.</summary>
    public bool PainfulWalkingForward { get; internal set; }

    /// <summary>Alternates painful forward and backward walk requests, advancing the stage only after its pause expires.</summary>
    internal bool StepPainfulWalking()
    {
        // The game stores four separate functions (request forward, wait forward,
        // request backward, wait backward). Two booleans express the same state without
        // hiding when a body program is actually installed.
        if (PainfulWalkingFunctionTimer != 0)
        {
            if (--PainfulWalkingFunctionTimer == 0)
            {
                PainfulWalkingStage++;
                PainfulWalkingForward = !PainfulWalkingForward;
            }
            return false;
        }

        MotherBrainWalkResult walk = PainfulWalkingForward
            ? _state.RequestWalkForward(72, PainfulWalkingAnimationDelay)
            : _state.RequestWalkBackward(40, PainfulWalkingAnimationDelay);
        if (walk.ReachedTarget)
        {
            int timerIndex = Math.Min(PainfulWalkingStage, 7);
            PainfulWalkingFunctionTimer = MotherBrainPainfulWalking.FunctionTimer(timerIndex);
        }
        return walk.Requested;
    }

    /// <summary>
    /// Runs the second phase drained by baby taken aback body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyTakenAback()
    {
        // The taken-aback step is not executed by the Baby. It runs on Mother Brain's next
        // actor turn, changes form/neck geometry, seeds 48, and falls through so
        // that this first regain-balance call immediately decrements it to 47.
        _state.Body.Form = MotherBrainForm.DrainedByBabyMetroid;
        _state.LowerNeckMovement = MotherBrainNeckMovement.Raise;
        _state.UpperNeckMovement = MotherBrainNeckMovement.Raise;
        _state.NeckAngleDelta = 1792;
        _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance;
        _state.FunctionTimer = 48;
        return true;
    }

    /// <summary>
    /// Runs the second phase drained by baby regain balance body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyRegainBalance(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.ColoursRequested = true;
        if (--_state.FunctionTimer < 0)
        {
            // The regain-balance step contains an apparent missing store: it loads one and then
            // immediately reloads the neck-enable flag. Preserve that no-op rather
            // than silently "fixing" it.
            PainfulWalkingForward = true;
            PainfulWalkingStage = 0;
            PainfulWalkingAnimationDelay = 2;
            PainfulWalkingFunctionTimer = 0;
            _state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
            _state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby firing rainbow beam body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyFiringRainbowBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        // The rainbow step refreshes the shake timer only if another subsystem has cleared it.
        // The head animation owns any decrement; retaining the write gate avoids
        // fabricating a body-side countdown that does not exist in this step.
        if (_state.BrainMainShakeTimer == 0)
            _state.BrainMainShakeTimer = 50;
        requests.ColoursRequested = true;
        requests.BodyWalkRequested = StepPainfulWalking();

        // The outer function updates delay and neck delta after *every* nested call.
        // Stage six is therefore observed on the exact call which ends its prior
        // pause, before the stage-six forward walk has even begun.
        if (PainfulWalkingStage < MotherBrainPainfulWalkingDefinitions.StageCount)
        {
            PainfulWalkingAnimationDelay =
                MotherBrainPainfulWalking.AnimationDelay(PainfulWalkingStage);
            _state.NeckAngleDelta = MotherBrainPainfulWalking.NeckAngleDelta(PainfulWalkingStage);
        }
        if (PainfulWalkingStage == 6)
        {
            _state.RainbowBeamSoundPlaying = false;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut;
            requests.SoundQueued = true; // The rainbow-drain sound stops as the beam runs out.
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby rainbow beam run out body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyRainbowBeamRunOut(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.BodyWalkRequested = StepPainfulWalking();
        if (PainfulWalkingStage >= 8)
        {
            _state.NeckAngleDelta = 64;
            _state.LowerNeckMovement = MotherBrainNeckMovement.Raise;
            _state.UpperNeckMovement = MotherBrainNeckMovement.Raise;
            _state.SetHeadAnimation(MotherBrainHeadSegment.DyingDrool);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby move to back of room body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyMoveToBackOfRoom(MotherBrainRainbowBeamStepRequests requests)
    {
        // The walk step does not reload Y before the shared walk helper. The surrounding
        // sequence's final painful-animation selector is ten, so the observable
        // retail list is the really-slow backward program.
        MotherBrainWalkResult moveToBack = _state.RequestWalkBackwardReallySlow(targetX: 40);
        requests.BodyWalkRequested = moveToBack.Requested;
        if (moveToBack.ReachedTarget)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode;
            _state.UpperNeckMovement = MotherBrainNeckMovement.None;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby go into low power mode body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyGoIntoLowPowerMode(MotherBrainRainbowBeamStepRequests requests)
    {
        // The brain slot independently advances the one-way neck raises. The body
        // clears drool once both indices reach zero, before waiting for its pose.
        if (_state.LowerNeckMovement == MotherBrainNeckMovement.None &&
            _state.UpperNeckMovement == MotherBrainNeckMovement.None)
        {
            _state.DroolGenerationEnabled = false;
            if (_state.Body.Pose != MotherBrainBodyPose.Standing)
                return false;
            _state.Body.SetAnimation(MotherBrainBodySegment.CrouchFast);
            requests.BodyPostureRequested = true;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey;
            _state.FunctionTimer = 64;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby prepare transition to grey body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyPrepareTransitionToGrey()
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.GreyTransitionCounter = 0;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey;
            _state.FunctionTimer = 16;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby transition to grey body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyTransitionToGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.FunctionTimer = 16;

            // The grey-transition list has eight nonzero palette entries followed by an end marker.
            // The caller increments its counter, passes the old index, and treats
            // the ninth (index-eight) probe as completion without copying colours.
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.ColoursRequested = paletteIndex < 8;
            if (paletteIndex >= 8)
            {
                _state.BrainHealth = 36000;
                _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Corpse;
                _state.DisableSmallPurpleBreathGeneration();
                _state.Body.Form = MotherBrainForm.SecondPhase;
                _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey;
            }
        }
        return false;
    }
}
