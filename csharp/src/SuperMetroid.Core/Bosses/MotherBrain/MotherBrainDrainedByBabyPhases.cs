namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain drained by the Baby Metroid: taken aback, firing back, running out, retreating and turning gray.</summary>
internal sealed class MotherBrainDrainedByBabyPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainDrainedByBabyPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>Current zero-based painful-walk stage at WRAM <c>$7E:802A</c>, 0 through 8.</summary>
    [IntegerQuantity("zero-based count of painful-walk steps taken, 0 through 8")]
    public int PainfulWalkingStage { get; internal set; }

    /// <summary>Walk animation delay derived from the current stage: 2, 4, 6, 8 or 10 updates.</summary>
    public int PainfulWalkingAnimationDelay { get; internal set; }

    /// <summary>Pause timer used after each completed forward/backward body list.</summary>
    public WrappingCountdown PainfulWalkingFunctionTimer { get; private set; }

    /// <summary>True while the nested painful-walk function is on its forward half.</summary>
    public bool PainfulWalkingForward { get; internal set; }

    /// <summary>Alternates painful forward and backward walk requests, advancing the stage only after its pause expires.</summary>
    internal bool StepPainfulWalking()
    {
        // The native stores four separate function pointers (request forward, wait forward,
        // request backward, wait backward). Two booleans express the same state without
        // hiding when a body instruction list is actually installed.
        if (!PainfulWalkingFunctionTimer.IsZero)
        {
            PainfulWalkingFunctionTimer = PainfulWalkingFunctionTimer.Decrement().Remaining;
            if (PainfulWalkingFunctionTimer.IsZero)
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
            PainfulWalkingFunctionTimer = WrappingCountdown.Start(MotherBrainPainfulWalkingDefinitions.FunctionTimer(timerIndex));
        }
        return walk.Requested;
    }

    /// <summary>
    /// Runs the second phase drained by baby taken aback body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyTakenAback()
    {
        // `$A9:BE38` is not executed by the Baby. It runs on Mother Brain's next
        // actor turn, changes form/neck geometry, seeds `$30`, and falls through so
        // that this first regain-balance call immediately decrements it to `$2F`.
        _state.Body.Form = MotherBrainForm.DrainedByBabyMetroid;
        _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
        _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Raise;
        _state.NeckAngleDelta = 1792;
        _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance;
        _state.FunctionTimer = WrappingCountdown.Start(48);
        return true;
    }

    /// <summary>
    /// Runs the second phase drained by baby regain balance body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyRegainBalance(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.PaletteRequested = true;
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            // `$BE80-$BE83` contains an apparent missing STA: it loads one and then
            // immediately reloads the neck-enable flag. Preserve that no-op rather
            // than silently "fixing" the cartridge.
            PainfulWalkingForward = true;
            PainfulWalkingStage = 0;
            PainfulWalkingAnimationDelay = 2;
            PainfulWalkingFunctionTimer = WrappingCountdown.Start(0);
            _state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby firing rainbow beam body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyFiringRainbowBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$BE96` refreshes the shake timer only if another subsystem has cleared it.
        // The head animation owns any decrement; retaining the write gate avoids
        // fabricating a body-side countdown that does not exist in this routine.
        if (_state.BrainMainShakeTimer.IsZero)
            _state.BrainMainShakeTimer = WrappingCountdown.Start(50);
        requests.PaletteRequested = true;
        requests.BodyWalkRequested = StepPainfulWalking();

        // The outer function updates delay and neck delta after *every* nested call.
        // Stage six is therefore observed on the exact call which ends its prior
        // pause, before the stage-six forward walk has even begun.
        if (PainfulWalkingStage < MotherBrainPainfulWalkingDefinitions.StageCount)
        {
            PainfulWalkingAnimationDelay =
                MotherBrainPainfulWalkingDefinitions.AnimationDelay(PainfulWalkingStage);
            _state.NeckAngleDelta = MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(PainfulWalkingStage);
        }
        if (PainfulWalkingStage == 6)
        {
            _state.RainbowBeamSoundPlaying = false;
            _state.BrainPaletteHandlingEnabled = false;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut;
            requests.SoundQueued = true; // Sound library one, effect `$02`.
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby rainbow beam run out body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyRainbowBeamRunOut(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.BodyWalkRequested = StepPainfulWalking();
        if (PainfulWalkingStage >= 8)
        {
            _state.NeckAngleDelta = 64;
            _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Raise;
            _state.SetHeadProgram(MotherBrainHeadProgram.DyingDrool);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby move to back of room body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyMoveToBackOfRoom(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$BF41` does not reload Y before the shared walk helper. The surrounding
        // sequence's final painful-animation selector is `$000A`, so the observable
        // retail list is the really-slow backward program at `$993A`.
        MotherBrainWalkResult moveToBack = _state.RequestWalkBackwardReallySlow(targetX: 40);
        requests.BodyWalkRequested = moveToBack.Requested;
        if (moveToBack.ReachedTarget)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.None;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby go into low power mode body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyGoIntoLowPowerMode(MotherBrainRainbowBeamStepRequests requests)
    {
        // The brain slot independently advances the one-way neck raises. The body
        // clears drool once both indices reach zero, before waiting for its pose.
        if (_state.LowerNeckMovementIndex == MotherBrainNeckMovement.None &&
            _state.UpperNeckMovementIndex == MotherBrainNeckMovement.None)
        {
            _state.DroolGenerationEnabled = false;
            if (_state.Body.Pose != MotherBrainBodyPose.Standing)
                return false;
            _state.Body.SetProgram(MotherBrainBodyProgram.CrouchFast);
            requests.BodyPostureRequested = true;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey;
            _state.FunctionTimer = WrappingCountdown.Start(64);
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby prepare transition to grey body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyPrepareTransitionToGrey()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.GreyTransitionCounter = 0;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey;
            _state.FunctionTimer = WrappingCountdown.Start(16);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase drained by baby transition to grey body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseDrainedByBabyTransitionToGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.FunctionTimer = WrappingCountdown.Start(16);

            // Bank `$AD:EF4A` has eight nonzero palette pointers followed by zero.
            // The caller increments its counter, passes the old index, and treats
            // the ninth (index-eight) probe as completion without copying colours.
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.PaletteRequested = paletteIndex < 8;
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
