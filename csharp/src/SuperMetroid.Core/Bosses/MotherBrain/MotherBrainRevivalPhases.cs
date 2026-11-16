namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain reviving and killing the Baby Metroid: signs of life, waking, walking up and the attacks on the baby.</summary>
internal sealed class MotherBrainRevivalPhases
{
    /// <summary>The late fight's own state, with the walking and posture helpers every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRevivalPhases(MotherBrainLateFightState state)
    {
        _state = state;
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
        Encounter.RevivalPhase = MotherBrainRevivalPhase.ShowSignsOfLife;
        Encounter.PhaseTimer.Count = 768;
        return false;
    }

    /// <summary>
    /// Runs the second phase revive show signs of life body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveShowSignsOfLife()
    {
        // The count of 768 lasts 769 calls. As it runs out, revival re-enables both breath producers,
        // seeds 224, and falls through two phase changes to the transition from grey.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.SmallPurpleBreathGenerationEnabled = true;
            Encounter.DroolGenerationEnabled = true;
            Encounter.RevivalPhase = MotherBrainRevivalPhase.TransitionFromGrey;
            Encounter.PhaseTimer.Count = 224;
            Encounter.GrayTransitionCounter = 0;
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
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.PhaseTimer.Count = 16;

            // The revival reverses the same eight grey fade steps used by the corpse
            // transition and follows them with zero. The body counter is incremented
            // before the old index is passed, so index eight is a terminating probe
            // with no copy. The existing result flag exposes each real copy cadence.
            int greyStep = Encounter.GrayTransitionCounter;
            Encounter.GrayTransitionCounter++;
            if (greyStep < 8)
                requests.RevivalColourStep = greyStep;
            if (greyStep >= 8)
            {
                Encounter.RevivalPhase = MotherBrainRevivalPhase.WakeUp;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive wake up body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWakeUp()
    {
        // The standing helper reports her standing only when the standing pose was already visible at entry.
        // A corpse pose therefore starts the stand-up animation and waits for the ordinary body
        // animation stage to publish standing before setting the neck and timer values.
        bool stoodUp = _state.MakeBodyStandUp();
        if (stoodUp)
        {
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
            Encounter.NeckMovementEnabled = true;
            Encounter.RevivalPhase = MotherBrainRevivalPhase.WakeUpStretch;
            Encounter.PhaseTimer.Count = 16;
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
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.StretchingPhase3);
            Encounter.RevivalPhase = MotherBrainRevivalPhase.WalkUpToBaby;
            Encounter.PhaseTimer.Count = 128;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive walk up to baby body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveWalkUpToBaby()
    {
        // Once the 128-count timer runs out, it stays run out and the
        // helper is retried after every body-animation call until X has overshot 80.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.RequestWalkForward(80, 4);
            bool reachedWalkTarget = CoordinateAxis.Difference(80, _state.BodyPosition.X) < 0 ||
                MotherBrainLateFightState.AtLeastBySignedDifference(_state.BodyPosition.X, 128);
            if (reachedWalkTarget)
            {
                _state.Phase2CorpseState = MotherBrainPhase2CorpseState.Revived;
                _state.HealthColoursEnabled = true;
                Encounter.RevivalPhase = MotherBrainRevivalPhase.PrepareNeckForBabyDeath;
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
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.BobDown;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.BobUp;
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Slow;
        Encounter.RevivalPhase = MotherBrainRevivalPhase.FinishPreparingForBabyDeath;
        return true;
    }

    /// <summary>
    /// Runs the second phase revive finish preparing for baby death body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseReviveFinishPreparingForBabyDeath()
    {
        bool stoodUp = _state.MakeBodyStandUp();
        if (stoodUp)
        {
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttack;
            _state.RequestWalkForward(80, 10);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase murder baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttack(RainbowBeamAttackFrame frame)
    {
        _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
        if (frame.RandomNumberSeed.IsNegative)
        {
            // A nonzero Baby enemy index selects the Baby-targeted volley; zero instead targets Samus.
            // This cutscene owns a spawned Baby, so retain the live actor
            // condition instead of unconditionally substituting the desired animation.
            _state.SetHeadAnimation(_state.BabyMetroidSpawned
                ? MotherBrainHeadSegment.AttackingBabyMetroid
                : MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttackCooldown;
            Encounter.PhaseTimer.Count = 64;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase murder baby attack cooldown body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttackCooldown()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttack;
        return false;
    }

    /// <summary>
    /// Runs the second phase prepare for final baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhasePrepareForFinalBabyAttack()
    {
        // This phase has no state transition. It requests stand-up and a
        // fast backward walk toward X 64 every call until the Baby overwrites it.
        _state.MakeBodyStandUp();
        _state.RequestWalkBackward(64, 4);
        return false;
    }

    /// <summary>
    /// Runs the second phase execute final baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseExecuteFinalBabyAttack()
    {
        _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingBabyMetroid);
        Encounter.RevivalPhase = MotherBrainRevivalPhase.FinalBabyAttackHolding;
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

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainRevivalPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it within the same call.
    /// </summary>
    internal bool Run(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests) => Encounter.RevivalPhase switch
    {
        MotherBrainRevivalPhase.InanimateGrey => RunSecondPhaseReviveInanimateGrey(),
        MotherBrainRevivalPhase.ShowSignsOfLife => RunSecondPhaseReviveShowSignsOfLife(),
        MotherBrainRevivalPhase.TransitionFromGrey => RunSecondPhaseReviveTransitionFromGrey(requests),
        MotherBrainRevivalPhase.WakeUp => RunSecondPhaseReviveWakeUp(),
        MotherBrainRevivalPhase.WakeUpStretch => RunSecondPhaseReviveWakeUpStretch(),
        MotherBrainRevivalPhase.WalkUpToBaby => RunSecondPhaseReviveWalkUpToBaby(),
        MotherBrainRevivalPhase.PrepareNeckForBabyDeath => RunSecondPhaseRevivePrepareNeckForBabyDeath(),
        MotherBrainRevivalPhase.FinishPreparingForBabyDeath => RunSecondPhaseReviveFinishPreparingForBabyDeath(),
        MotherBrainRevivalPhase.MurderBabyAttack => RunSecondPhaseMurderBabyAttack(frame),
        MotherBrainRevivalPhase.MurderBabyAttackCooldown => RunSecondPhaseMurderBabyAttackCooldown(),
        MotherBrainRevivalPhase.PrepareForFinalBabyAttack => RunSecondPhasePrepareForFinalBabyAttack(),
        MotherBrainRevivalPhase.ExecuteFinalBabyAttack => RunSecondPhaseExecuteFinalBabyAttack(),
        MotherBrainRevivalPhase.FinalBabyAttackHolding => RunSecondPhaseFinalBabyAttackHolding(),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainRevivalPhase)} {Encounter.RevivalPhase}."),
    };
}
