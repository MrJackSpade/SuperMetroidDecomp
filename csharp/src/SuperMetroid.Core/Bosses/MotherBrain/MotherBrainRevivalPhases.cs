namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain reviving and killing the Baby Metroid: signs of life, waking, walking up and the attacks on the baby.</summary>
internal sealed class MotherBrainRevivalPhases
{
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainRevivalPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>
    /// How many four-ring attacks Mother Brain has begun against the Baby, zero through twelve, where it stays; it also
    /// chooses the pitch of her cry.
    /// </summary>
    public int BabyMetroidAttackCounter { get; internal set; }

    /// <summary>
    /// Runs the second phase revive inanimate grey body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseReviveInanimateGrey()
    {
        // The grey body only moves on to its signs of life, with a timer of 768 whose first step comes on the
        // next update.
        Encounter.RevivalPhase = MotherBrainRevivalPhase.ShowSignsOfLife;
        Encounter.PhaseTimer.Count = 768;
        return false;
    }

    /// <summary>
    /// Runs the second phase revive show signs of life body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseReviveShowSignsOfLife()
    {
        // The count of 768 lasts 769 updates. As it runs out, her breath and drool start again and the transition from
        // grey starts at 224, in this same update.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.SmallPurpleBreathGenerationEnabled = true;
            Encounter.DroolGenerationEnabled = true;
            Encounter.RevivalPhase = MotherBrainRevivalPhase.TransitionFromGrey;
            Encounter.PhaseTimer.Count = 224;
            Encounter.RevivalFromGrey.Restart();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase revive transition from grey body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseReviveTransitionFromGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.PhaseTimer.Count = 16;

            // The revival reverses the same eight greying steps used by the corpse transition. Its ninth step shows no
            // new colours and completes the revival into her waking up.
            int greyStep = Encounter.RevivalFromGrey.TakeStep();
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseReviveWakeUp()
    {
        // She counts as standing only once her standing pose already shows. From her corpse pose she starts
        // standing up, and the neck and timer are set once her body animation shows her standing.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseReviveWalkUpToBaby()
    {
        // Once the timer of 128 runs out it stays run out, and she asks to walk forward on every update until she
        // is past X 80.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.RequestWalkForward(80, 4);
            // Past X 80 she has overshot the walk's target; the walk's limit at X 128 lies beyond it.
            bool reachedWalkTarget = _state.BodyPosition.X > 80;
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseRevivePrepareNeckForBabyDeath()
    {
        // Readying the neck for the Baby's death takes one update and moves on in that same update; her body may
        // still be finishing the last walk.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttack(RainbowBeamAttackFrame frame)
    {
        _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
        if (frame.RandomNumberSeed.IsNegative)
        {
            // She fires her volley at the Baby once the Baby has appeared; before that, at Samus.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseMurderBabyAttackCooldown()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttack;
        return false;
    }

    /// <summary>
    /// Runs the second phase prepare for final baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhasePrepareForFinalBabyAttack()
    {
        // This phase never moves on itself: it asks her to stand and walk back fast toward X 64 on every update until
        // the Baby moves her on.
        _state.MakeBodyStandUp();
        _state.RequestWalkBackward(64, 4);
        return false;
    }

    /// <summary>
    /// Runs the second phase execute final baby attack body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunSecondPhaseExecuteFinalBabyAttack()
    {
        _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingBabyMetroid);
        Encounter.RevivalPhase = MotherBrainRevivalPhase.FinalBabyAttackHolding;
        return false;
    }

    /// <summary>
    /// Runs the second phase final baby attack holding body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal static bool RunSecondPhaseFinalBabyAttackHolding()
    {
        // The final attack's holding step is empty. The last ring volley owns the remaining
        // health reduction and the Baby actor owns its death/recovery sequence.
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainRevivalPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
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
