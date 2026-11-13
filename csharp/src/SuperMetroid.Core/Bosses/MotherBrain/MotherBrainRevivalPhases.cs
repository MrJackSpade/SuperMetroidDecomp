namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain reviving and killing the Baby Metroid: signs of life, waking, walking up and the attacks on the baby.</summary>
internal sealed class MotherBrainRevivalPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body phase and its timer, the neck and head animations and the brain's flags, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRevivalPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>Records a body-AI write to the breath flag so a later brain-animation cue can remain a separate writer.</summary>
    private void EnableSmallPurpleBreathGeneration()
    {
        _state.SmallPurpleBreathGenerationEnabled = true;
        _state.SmallPurpleBreathGenerationWriteCount++;
    }

    /// <summary>
    /// Saturating 0..12 counter incremented by the head's attack-counter step at the start
    /// of each four-ring attack against the Baby. It also selects the intended cry pitch.
    /// </summary>
    public int BabyMetroidAttackCounter { get; internal set; }

    /// <summary>
    /// Runs the second phase revive inanimate grey body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveInanimateGrey()
    {
        // The inanimate setup only moves to the next phase and sets its 768-frame timer. It does not
        // fall through, so the first pre-decrement belongs to the following call.
        _state.Phase = MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife;
        _state.PhaseTimer = 768;
        return false;
    }

    /// <summary>
    /// Runs the second phase revive show signs of life body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveShowSignsOfLife()
    {
        // The below-zero check accepts zero: 768 reaches zero without advancing and only the 769th
        // call passes zero to -1. Revival then re-enables both breath producers,
        // seeds 224, and falls through two phase changes to the transition from grey.
        if (--_state.PhaseTimer < 0)
        {
            EnableSmallPurpleBreathGeneration();
            _state.DroolGenerationEnabled = true;
            _state.Phase = MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey;
            _state.PhaseTimer = 224;
            _state.GreyTransitionCounter = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive transition from grey body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveTransitionFromGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.PhaseTimer < 0)
        {
            _state.PhaseTimer = 16;

            // The revival palette list reverses the same eight grey palettes used by the corpse
            // transition and follows them with zero. The body counter is incremented
            // before the old index is passed, so index eight is a terminating probe
            // with no copy. The existing result flag exposes each real copy cadence.
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.ColoursRequested = paletteIndex < 8;
            if (paletteIndex >= 8)
            {
                _state.Phase = MotherBrainBodyPhase.SecondPhaseReviveWakeUp;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive wake up body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWakeUp(MotherBrainRainbowBeamStepRequests requests)
    {
        // The standing helper reports her standing only when the standing pose was already visible at entry.
        // A corpse pose therefore starts the stand-up animation and waits for the ordinary body
        // animation stage to publish standing before setting the neck and timer values.
        bool stoodUp = _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        if (stoodUp)
        {
            _state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            _state.NeckMovementEnabled = true;
            _state.Phase = MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch;
            _state.PhaseTimer = 16;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive wake up stretch body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWakeUpStretch()
    {
        if (--_state.PhaseTimer < 0)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.StretchingPhase3);
            _state.Phase = MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby;
            _state.PhaseTimer = 128;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive walk up to baby body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWalkUpToBaby(MotherBrainRainbowBeamStepRequests requests)
    {
        // Once the 128-count timer passes zero, the timer deliberately remains negative and the
        // helper is retried after every body-animation call until X has overshot 80.
        if (--_state.PhaseTimer < 0)
        {
            requests.BodyWalkRequested = _state.RequestWalkForward(80, 4).Requested;
            bool reachedWalkTarget = CoordinateAxis.Difference(80, _state.Body.Position.X) < 0 ||
                MotherBrainLateFightState.AtLeastBySignedDifference(_state.Body.Position.X, 128);
            if (reachedWalkTarget)
            {
                _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Revived;
                _state.HealthColoursEnabled = true;
                _state.Phase = MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive prepare neck for baby death body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseRevivePrepareNeckForBabyDeath()
    {
        // The walk-up setup is a one-call setup phase which immediately falls into the walk.
        // The separate body animation stage may still be finishing the last walk.
        BabyMetroidAttackCounter = 0;
        _state.NeckMovementEnabled = true;
        _state.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        _state.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        _state.NeckAngleDelta = 64;
        _state.Phase = MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath;
        return true;
    }

    /// <summary>
    /// Runs the second phase revive finish preparing for baby death body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveFinishPreparingForBabyDeath(MotherBrainRainbowBeamStepRequests requests)
    {
        bool stoodUp = _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        if (stoodUp)
        {
            _state.Phase = MotherBrainBodyPhase.SecondPhaseMurderBabyAttack;
            requests.BodyWalkRequested = _state.RequestWalkForward(80, 10).Requested;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase murder baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttack(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        requests.BodyPostureRequested = _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
        if (frame.RandomNumberSeed.IsNegative)
        {
            // A nonzero Baby enemy index selects the Baby-targeted volley; zero instead targets Samus.
            // This cutscene owns a spawned Baby, so retain the live actor
            // condition instead of unconditionally substituting the desired animation.
            _state.SetHeadAnimation(_state.BabyMetroidSpawned
                ? MotherBrainHeadSegment.AttackingBabyMetroid
                : MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
            _state.Phase = MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown;
            _state.PhaseTimer = 64;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase murder baby attack cooldown body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttackCooldown()
    {
        if (--_state.PhaseTimer < 0)
            _state.Phase = MotherBrainBodyPhase.SecondPhaseMurderBabyAttack;
        return false;
    }

    /// <summary>
    /// Runs the second phase prepare for final baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhasePrepareForFinalBabyAttack(MotherBrainRainbowBeamStepRequests requests)
    {
        // This phase has no state transition. It requests stand-up and a
        // fast backward walk toward X 64 every call until the Baby overwrites it.
        _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        requests.BodyWalkRequested |= _state.RequestWalkBackward(64, 4).Requested;
        return false;
    }

    /// <summary>
    /// Runs the second phase execute final baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseExecuteFinalBabyAttack()
    {
        _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingBabyMetroid);
        _state.Phase = MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding;
        return false;
    }

    /// <summary>
    /// Runs the second phase final baby attack holding body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunSecondPhaseFinalBabyAttackHolding()
    {
        // The final attack's holding step is empty. The last ring volley owns the remaining
        // health reduction and the Baby actor owns its death/recovery sequence.
        return false;
    }
}
