using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
internal sealed class MotherBrainFinishSamusOffPhases
{
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFinishSamusOffPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>
    /// The energy at or below which Mother Brain stands up to finish Samus off:
    /// <see cref="MotherBrainFinishOffDefinitions.StandUpOnionRingHits"/> onion ring hits as Samus's suits divide them,
    /// plus <see cref="MotherBrainFinishOffDefinitions.EnergyMargin"/>.
    /// </summary>
    internal static int StandUpToFinishOffEnergy(SamusEquipmentFlags equippedItems) =>
        SamusSuitDamage.Reduce(MotherBrainAttackDamage.OnionRing, equippedItems) * MotherBrainFinishOffDefinitions.StandUpOnionRingHits
        + MotherBrainFinishOffDefinitions.EnergyMargin;

    /// <summary>
    /// The energy at or below which Mother Brain keeps to onion rings rather than choosing her bomb: one bomb hit as
    /// Samus's suits divide it, plus <see cref="MotherBrainFinishOffDefinitions.EnergyMargin"/>.
    /// </summary>
    internal static int OnionRingsOnlyEnergy(SamusEquipmentFlags equippedItems) =>
        SamusSuitDamage.Reduce(MotherBrainAttackDamage.Bomb, equippedItems) + MotherBrainFinishOffDefinitions.EnergyMargin;

    /// <summary>Runs one update of readying the Baby Metroid's arrival, rejecting calls once it has arrived.</summary>
    private void ReadyBabyMetroidArrival()
    {
        if (_state.BabyArrivalReadyingElapsed >= MotherBrainReadyingDefinitions.BabyArrivalDuration)
            throw new InvalidOperationException("The Baby Metroid's arrival is already readied.");

        _state.BabyArrivalReadyingElapsed++;
    }

    /// <summary>
    /// Attacks Samus with onion rings and bombs until her energy is low enough to finish her off, then stands up. True
    /// when her next phase runs on in this same update.
    /// </summary>
    internal bool AttackUntilSamusIsLow(SamusState samus, RainbowBeamAttackFrame frame)
    {
        // The low-energy check asks whether a few more onion rings could put Samus at the encounter's intended
        // low-energy floor. Equality counts as low and changes the body phase; she stands up on the next update.
        if (StandUpToFinishOffEnergy(samus.EquippedItems) >= samus.Health)
        {
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.StandUp;
            return false;
        }

        // Above the floor, the hold-back chance does nothing. Every
        // 32nd enemy frame that idle route may ask her to alternate between standing and leaning down. This is visible body animation, not a
        // direct pose assignment, so retain the separate animation-stage delay.
        if (frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.FinishOffHoldsBack))
        {
            if (MotherBrainCadenceDefinitions.FinishOffPostureEveryThirtySecondUpdate.IsDue(frame.EnemyFrameCounter))
                _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
            return false;
        }

        // A weaker one-bomb threshold biases wounded Samus toward onion rings. Only when Samus is above it does the
        // bomb's chance choose a bomb; all remaining admitted values still choose two onion rings.
        if (OnionRingsOnlyEnergy(samus.EquippedItems) >= samus.Health ||
            !frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.FinishOffBombs))
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.TwoOnionRingsPhase2);
        }
        else
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.BombPhase2);
        }
        return false;
    }

    /// <summary>
    /// Stands her up, then starts her admiring of the beaten Samus. True when her next phase runs on in this same
    /// update.
    /// </summary>
    internal bool StandUpToFinishOff()
    {
        // She counts as standing only if she already stood as the update began.
        // Crouched/leaning poses request their matching stand animation and report her not
        // yet standing; walking/transition poses simply wait for their current animation.
        bool stoodUp = _state.MakeBodyStandUp();
        if (stoodUp)
        {
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.Admire;
            Encounter.PhaseTimer.LastFor(MotherBrainWaitDefinitions.AdmireUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Admires the beaten Samus until the wait runs out, then stretches and starts charging the final beam. True when
    /// her next phase runs on in this same update.
    /// </summary>
    internal bool AdmireSamus()
    {
        // Stand-up falls through here, so the admiration's first update is the update she stands up on.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.StretchingPhase2);
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.ChargeFinalBeam;
            Encounter.PhaseTimer.LastFor(MotherBrainWaitDefinitions.FinalBeamChargeUpdates);
        }
        return false;
    }

    /// <summary>
    /// Charges the final beam until the wait runs out. True when her next phase runs on in this same update.
    /// </summary>
    internal bool ChargeFinalBeam()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.ChargingRainbowBeam);
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.ReadyBaby;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Readies the Baby's arrival one update at a time; the last update spawns it and starts the final beam's wait.
    /// True when her next phase runs on in this same update.
    /// </summary>
    internal bool BringInBaby(MotherBrainRainbowBeamStepRequests requests)
    {
        // Each update readies one update of the Baby's arrival; the last spawns it on that same update.
        ReadyBabyMetroidArrival();
        if (_state.BabyArrivalReadyingElapsed == MotherBrainReadyingDefinitions.BabyArrivalDuration)
        {
            _state.RetractHead();
            _state.BabyMetroidSpawned = true;
            requests.BabySpawnRequested = true;
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.FireFinalBeam;
            Encounter.PhaseTimer.LastFor(MotherBrainWaitDefinitions.FinalBeamFireDelayUpdates);
        }
        return false;
    }

    /// <summary>
    /// Fires the final beam when the wait runs out. True when her next phase runs on in this same update.
    /// </summary>
    internal bool FireFinalBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.FiringRainbowBeam);
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
            requests.FinalBeamFired = true; // The final beam's sound, and her rainbow colours start again from the first.
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.FinalBeamHolding;
        }
        return false;
    }

    /// <summary>
    /// Holds the final beam until the Baby latches onto her. True when her next phase runs on in this same update.
    /// </summary>
    internal static bool HoldFinalBeam()
    {
        // The final beam's holding step is empty. Mother Brain remains here
        // until the independently spawned Baby actor overwrites her body phase
        // with the taken-aback step after latching onto her head.
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainFinishOffPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it in the same update.
    /// </summary>
    internal bool Run(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests) => Encounter.FinishOffPhase switch
    {
        MotherBrainFinishOffPhase.GetSamusToLowEnergy => AttackUntilSamusIsLow(samus, frame),
        MotherBrainFinishOffPhase.StandUp => StandUpToFinishOff(),
        MotherBrainFinishOffPhase.Admire => AdmireSamus(),
        MotherBrainFinishOffPhase.ChargeFinalBeam => ChargeFinalBeam(),
        MotherBrainFinishOffPhase.ReadyBaby => BringInBaby(requests),
        MotherBrainFinishOffPhase.FireFinalBeam => FireFinalBeam(requests),
        MotherBrainFinishOffPhase.FinalBeamHolding => HoldFinalBeam(),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainFinishOffPhase)} {Encounter.FinishOffPhase}."),
    };
}
