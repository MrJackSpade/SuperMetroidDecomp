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
    /// How many four-ring attacks Mother Brain has begun against the Baby, zero through twelve, where it stays; her
    /// eleventh attack is made without her attack cry (<see cref="MotherBrainFightDefinitions.SilentBabyAttackCount"/>).
    /// </summary>
    public int BabyMetroidAttackCounter { get; internal set; }

    /// <summary>
    /// Starts her inanimate wait as a grey corpse; the wait starts on the next update. True when her next phase runs on
    /// in this same update.
    /// </summary>
    internal bool LieInanimate()
    {
        // The grey body only moves on to its signs of life, whose wait starts on the next update.
        Encounter.RevivalPhase = MotherBrainRevivalPhase.ShowSignsOfLife;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.InanimateUpdates);
        return false;
    }

    /// <summary>
    /// As her inanimate wait runs out, starts her breath and drool again and her colour's return. True when her next
    /// phase runs on in this same update.
    /// </summary>
    internal bool ShowSignsOfLife()
    {
        // As her inanimate wait runs out, her breath and drool start again and the transition from grey starts, its first
        // update this same update.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.SmallPurpleBreathGenerationEnabled = true;
            Encounter.DroolGenerationEnabled = true;
            Encounter.RevivalPhase = MotherBrainRevivalPhase.TransitionFromGrey;
            Encounter.RevivalFromGrey.StartAfter(MotherBrainLateFightDefinitions.RevivalFirstGreyStepUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Brings her colour back through the revival's fade; the update the fade is over wakes her. True when her next phase
    /// runs on in this same update.
    /// </summary>
    internal bool RegainColour(MotherBrainRainbowBeamStepRequests requests)
    {
        // The revival reverses the greying the drain showed; the update after its last image's hold shows no new colours
        // and completes the revival into her waking up.
        if (Encounter.RevivalFromGrey.Step() is { } image)
            requests.RevivalColourImage = image;
        else if (Encounter.RevivalFromGrey.IsOver)
        {
            Encounter.RevivalPhase = MotherBrainRevivalPhase.WakeUp;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Stands her up from her corpse pose, then lowers her neck and starts her stretch. True when her next phase runs
    /// on in this same update.
    /// </summary>
    internal bool WakeUp()
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
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.WakeUpStretchUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Stretches until the wait runs out, then starts the wait before she walks up to the Baby. True when her next
    /// phase runs on in this same update.
    /// </summary>
    internal bool Stretch()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.StretchingPhase3);
            Encounter.RevivalPhase = MotherBrainRevivalPhase.WalkUpToBaby;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.BeforeWalkUpToBabyUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Walks her forward fast until she is past the Baby's walk target, then her colours follow her health again. True
    /// when her next phase runs on in this same update.
    /// </summary>
    internal bool WalkUpToBaby()
    {
        // Once the wait has run out it stays run out, and she asks to walk forward on every update until she is past the
        // walk's target.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.RequestWalkForward(MotherBrainLateFightDefinitions.BabyWalkUpX, MotherBrainWalkSpeed.Fast);
            // Past the walk's target she has overshot it; the walk's front limit lies beyond it.
            bool reachedWalkTarget = _state.BodyPosition.X > MotherBrainLateFightDefinitions.BabyWalkUpX;
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
    /// Starts her neck's slow bob and her count of attacks on the Baby. True when her next phase runs on in this same
    /// update.
    /// </summary>
    internal bool BobNeckToAttackBaby()
    {
        // Starting the neck's bob for the Baby's death takes one update and moves on in that same update; her body may
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
    /// Stands her up, then starts her really slow walk and her attacks. True when her next phase runs on in this same
    /// update.
    /// </summary>
    internal bool StandToAttackBaby()
    {
        bool stoodUp = _state.MakeBodyStandUp();
        if (stoodUp)
        {
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttack;
            _state.RequestWalkForward(MotherBrainLateFightDefinitions.BabyWalkUpX, MotherBrainWalkSpeed.ReallySlow);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Fires her volley at the Baby, or at Samus before the Baby appears, when her chance to fire comes. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool AttackBaby(RainbowBeamAttackFrame frame)
    {
        _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
        if (frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.BabyVolleyFires))
        {
            // She fires her volley at the Baby once the Baby has appeared; before that, at Samus.
            _state.SetHeadAnimation(_state.BabyMetroidSpawned
                ? MotherBrainHeadSegment.AttackingBabyMetroid
                : MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3);
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttackCooldown;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.BabyAttackCooldownUpdates);
        }
        return false;
    }

    /// <summary>
    /// Waits out the attack's cooldown, then attacks again. True when her next phase runs on in this same update.
    /// </summary>
    internal bool CoolDownFromBabyAttack()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.RevivalPhase = MotherBrainRevivalPhase.MurderBabyAttack;
        return false;
    }

    /// <summary>
    /// Stands her and walks her back fast toward her retreat until the Baby moves her on. True when her next phase runs
    /// on in this same update.
    /// </summary>
    internal bool BackAwayForFinalBabyAttack()
    {
        // This phase never moves on itself: it asks her to stand and walk back fast toward its retreat on every update until
        // the Baby moves her on.
        _state.MakeBodyStandUp();
        _state.RequestWalkBackward(MotherBrainLateFightDefinitions.FinalBabyAttackRetreatX, MotherBrainWalkSpeed.Fast);
        return false;
    }

    /// <summary>
    /// Fires her last volley at the Baby. True when her next phase runs on in this same update.
    /// </summary>
    internal bool FireFinalBabyAttack()
    {
        _state.SetHeadAnimation(MotherBrainHeadSegment.AttackingBabyMetroid);
        Encounter.RevivalPhase = MotherBrainRevivalPhase.FinalBabyAttackHolding;
        return false;
    }

    /// <summary>
    /// Holds after her last volley while the volley and the Baby finish. True when her next phase runs on in this same
    /// update.
    /// </summary>
    internal static bool HoldFinalBabyAttack()
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
        MotherBrainRevivalPhase.InanimateGrey => LieInanimate(),
        MotherBrainRevivalPhase.ShowSignsOfLife => ShowSignsOfLife(),
        MotherBrainRevivalPhase.TransitionFromGrey => RegainColour(requests),
        MotherBrainRevivalPhase.WakeUp => WakeUp(),
        MotherBrainRevivalPhase.WakeUpStretch => Stretch(),
        MotherBrainRevivalPhase.WalkUpToBaby => WalkUpToBaby(),
        MotherBrainRevivalPhase.PrepareNeckForBabyDeath => BobNeckToAttackBaby(),
        MotherBrainRevivalPhase.FinishPreparingForBabyDeath => StandToAttackBaby(),
        MotherBrainRevivalPhase.MurderBabyAttack => AttackBaby(frame),
        MotherBrainRevivalPhase.MurderBabyAttackCooldown => CoolDownFromBabyAttack(),
        MotherBrainRevivalPhase.PrepareForFinalBabyAttack => BackAwayForFinalBabyAttack(),
        MotherBrainRevivalPhase.ExecuteFinalBabyAttack => FireFinalBabyAttack(),
        MotherBrainRevivalPhase.FinalBabyAttackHolding => HoldFinalBabyAttack(),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainRevivalPhase)} {Encounter.RevivalPhase}."),
    };
}
