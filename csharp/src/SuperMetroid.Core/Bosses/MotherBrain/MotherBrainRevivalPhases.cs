namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain reviving and killing the Baby Metroid: signs of life, waking, walking up and the attacks on the baby.</summary>
internal sealed class MotherBrainRevivalPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRevivalPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>Records a body-AI write to the breath flag so a later brain-list opcode can remain a separate writer.</summary>
    private void EnableSmallPurpleBreathGeneration()
    {
        _state.SmallPurpleBreathGenerationEnabled = true;
        _state.SmallPurpleBreathGenerationWriteCount++;
    }

    /// <summary>
    /// Saturating `$00-$0C` counter incremented by head instruction `$A9:9EA3` at the start
    /// of each four-ring attack against the Baby. It also selects the intended cry pitch.
    /// </summary>
    public int BabyMetroidAttackCounter { get; internal set; }

    /// <summary>
    /// Runs the second phase revive inanimate grey body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveInanimateGrey()
    {
        // `$C059` only installs the next function and its `$0300` timer. It does not
        // fall through, so the first pre-decrement belongs to the following call.
        _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife;
        _state.FunctionTimer = WrappingCountdown.Start(768);
        return false;
    }

    /// <summary>
    /// Runs the second phase revive show signs of life body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveShowSignsOfLife()
    {
        // BPL accepts zero: 768 reaches zero without advancing and only the 769th
        // call underflows to `$FFFF`. Revival then re-enables both breath producers,
        // seeds `$E0`, and falls through two function installations to `$C08F`.
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            EnableSmallPurpleBreathGeneration();
            _state.DroolGenerationEnabled = true;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey;
            _state.FunctionTimer = WrappingCountdown.Start(224);
            _state.GreyTransitionCounter = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive transition from grey body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveTransitionFromGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.FunctionTimer = WrappingCountdown.Start(16);

            // `$AD:ED9C` reverses the same eight grey palettes used by the corpse
            // transition and follows them with zero. The body counter is incremented
            // before the old index is passed, so index eight is a terminating probe
            // with no copy. The existing result flag exposes each real copy cadence.
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.PaletteRequested = paletteIndex < 8;
            if (paletteIndex >= 8)
            {
                _state.BrainPaletteHandlingEnabled = true;
                _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveWakeUp;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive wake up body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWakeUp(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$C670` returns carry only when pose zero was already visible at entry.
        // A corpse pose therefore installs `$99C6` and waits for the ordinary body
        // instruction stage to publish standing before setting the neck/timer words.
        bool stoodUp = _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        if (stoodUp)
        {
            _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            _state.NeckMovementEnabled = true;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch;
            _state.FunctionTimer = WrappingCountdown.Start(16);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive wake up stretch body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWakeUpStretch()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.StretchingPhase3);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby;
            _state.FunctionTimer = WrappingCountdown.Start(128);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive walk up to baby body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWalkUpToBaby(MotherBrainRainbowBeamStepRequests requests)
    {
        // Once `$80` underflows, the timer deliberately remains negative and the
        // helper is retried after every body-program call until X has overshot `$50`.
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            requests.BodyWalkRequested = _state.RequestWalkForward(80, 4).Requested;
            bool reachedWalkTarget = unchecked((short)(80 - _state.Body.Position.X)) < 0 ||
                MotherBrainLateFightState.AtLeastBySignedDifference(_state.Body.Position.X, 128);
            if (reachedWalkTarget)
            {
                _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Revived;
                _state.HealthBasedPaletteHandlingEnabled = true;
                _state.Phase = MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive prepare neck for baby death body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRevivePrepareNeckForBabyDeath()
    {
        // `$C11E` is a one-call setup function which immediately falls into `$C147`.
        // The separate body instruction stage may still be finishing the last walk.
        BabyMetroidAttackCounter = 0;
        _state.NeckMovementEnabled = true;
        _state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
        _state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        _state.NeckAngleDelta = 64;
        _state.Phase = MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath;
        return true;
    }

    /// <summary>
    /// Runs the second phase revive finish preparing for baby death body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveFinishPreparingForBabyDeath(MotherBrainRainbowBeamStepRequests requests)
    {
        bool stoodUp = _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        if (stoodUp)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseMurderBabyAttack;
            requests.BodyWalkRequested = _state.RequestWalkForward(80, 10).Requested;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase murder baby attack body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttack(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        requests.BodyPostureRequested = _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
        if (frame.RandomNumberSeed.IsNegative)
        {
            // A nonzero Baby enemy index selects `$9DB1`; zero instead targets Samus.
            // This translated cutscene owns a spawned Baby, so retain the live actor
            // condition instead of unconditionally substituting the desired list.
            _state.SetHeadProgram(_state.BabyMetroidSpawned
                ? MotherBrainHeadProgram.AttackingBabyMetroid
                : MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown;
            _state.FunctionTimer = WrappingCountdown.Start(64);
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase murder baby attack cooldown body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttackCooldown()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
            _state.Phase = MotherBrainBodyFunction.SecondPhaseMurderBabyAttack;
        return false;
    }

    /// <summary>
    /// Runs the second phase prepare for final baby attack body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhasePrepareForFinalBabyAttack(MotherBrainRainbowBeamStepRequests requests)
    {
        // This native function has no state transition. It requests stand-up and a
        // fast backward walk toward `$40` every call until the Baby overwrites it.
        _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        requests.BodyWalkRequested |= _state.RequestWalkBackward(64, 4).Requested;
        return false;
    }

    /// <summary>
    /// Runs the second phase execute final baby attack body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseExecuteFinalBabyAttack()
    {
        _state.SetHeadProgram(MotherBrainHeadProgram.AttackingBabyMetroid);
        _state.Phase = MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding;
        return false;
    }

    /// <summary>
    /// Runs the second phase final baby attack holding body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunSecondPhaseFinalBabyAttackHolding()
    {
        // `$C1A6` is the installed RTS. The last ring volley owns the remaining
        // health reduction and the Baby actor owns its death/recovery sequence.
        return false;
    }
}
