using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
internal sealed class MotherBrainFinishSamusOffPhases
{
    /// <summary>The late fight's own state, with the walking and posture helpers every late-fight phase shares.</summary>
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
    /// Runs the second phase finish samus off body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOff(SamusState samus, RainbowBeamAttackFrame frame)
    {
        // The low-energy check asks whether a few more onion rings could put Samus at the encounter's intended
        // low-energy floor. Equality counts as low and changes the body phase without falling into the stand-up
        // step on this call.
        if (StandUpToFinishOffEnergy(samus.EquippedItems) >= samus.Health)
        {
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.StandUp;
            return false;
        }

        // Above the floor, the hold-back chance does nothing. Every
        // 32nd enemy frame that idle route may ask the posture helper to alternate
        // between standing and leaning down. This is visible body animation, not a
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
    /// Runs the second phase finish samus off stand up body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffStandUp()
    {
        // The standing helper reports her standing only if she already stood at call entry.
        // Crouched/leaning poses request their matching stand animation and report her not
        // yet standing; walking/transition poses simply wait for their current animation.
        bool stoodUp = _state.MakeBodyStandUp();
        if (stoodUp)
        {
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.Admire;
            Encounter.PhaseTimer.Count = 16;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off admire body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffAdmire()
    {
        // Stand-up falls through here, so the freshly loaded countdown takes its first step on the same AI call.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.StretchingPhase2);
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.ChargeFinalBeam;
            Encounter.PhaseTimer.Count = 256;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off charge final beam body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffChargeFinalBeam()
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
    /// Readies the Baby Metroid's arrival as Mother Brain finishes Samus off, one update at a time; the last spawns the
    /// Baby and starts the final beam's wait. Never runs on into the next phase within the same update.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffReadyBaby(MotherBrainRainbowBeamStepRequests requests)
    {
        // Each call readies one update of the Baby's arrival; the last spawns it on that same call.
        ReadyBabyMetroidArrival();
        if (_state.BabyArrivalReadyingElapsed == MotherBrainReadyingDefinitions.BabyArrivalDuration)
        {
            _state.RetractHead();
            _state.BabyMetroidSpawned = true;
            requests.BabySpawnRequested = true;
            Encounter.FinishOffPhase = MotherBrainFinishOffPhase.FireFinalBeam;
            Encounter.PhaseTimer.Count = 256;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off fire final beam body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffFireFinalBeam(MotherBrainRainbowBeamStepRequests requests)
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
    /// Runs the second phase final rainbow beam holding body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunSecondPhaseFinalRainbowBeamHolding()
    {
        // The final beam's holding step is empty. Mother Brain remains here
        // until the independently spawned Baby actor overwrites her body phase
        // with the taken-aback step after latching onto her head.
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainFinishOffPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it within the same call.
    /// </summary>
    internal bool Run(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests) => Encounter.FinishOffPhase switch
    {
        MotherBrainFinishOffPhase.GetSamusToLowEnergy => RunSecondPhaseFinishSamusOff(samus, frame),
        MotherBrainFinishOffPhase.StandUp => RunSecondPhaseFinishSamusOffStandUp(),
        MotherBrainFinishOffPhase.Admire => RunSecondPhaseFinishSamusOffAdmire(),
        MotherBrainFinishOffPhase.ChargeFinalBeam => RunSecondPhaseFinishSamusOffChargeFinalBeam(),
        MotherBrainFinishOffPhase.ReadyBaby => RunSecondPhaseFinishSamusOffReadyBaby(requests),
        MotherBrainFinishOffPhase.FireFinalBeam => RunSecondPhaseFinishSamusOffFireFinalBeam(requests),
        MotherBrainFinishOffPhase.FinalBeamHolding => RunSecondPhaseFinalRainbowBeamHolding(),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainFinishOffPhase)} {Encounter.FinishOffPhase}."),
    };
}
