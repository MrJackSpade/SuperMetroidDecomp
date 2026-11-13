using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
internal sealed class MotherBrainFinishSamusOffPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head animations and the brain's flags, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

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
    /// Runs the second phase finish samus off body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOff(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The low-energy check asks whether a few more onion rings could put Samus at the encounter's intended
        // low-energy floor. Equality counts as low and changes the body function without falling into the stand-up
        // handler on this call.
        if (StandUpToFinishOffEnergy(samus.EquippedItems) >= samus.Health)
        {
            _state.Phase = MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp;
            return false;
        }

        // Above the floor, the hold-back chance does nothing. Every
        // 32nd enemy frame that idle route may ask the posture helper to alternate
        // between standing and leaning down. This is visible body animation, not a
        // direct pose assignment, so retain the separate animation-stage delay.
        if (frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.FinishOffHoldsBack))
        {
            if (MotherBrainCadenceDefinitions.FinishOffPostureEveryThirtySecondUpdate.IsDue(frame.EnemyFrameCounter))
                requests.BodyPostureRequested = _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
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
    /// Runs the second phase finish samus off stand up body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffStandUp(MotherBrainRainbowBeamStepRequests requests)
    {
        // The standing helper reports her standing only if she already stood at call entry.
        // Crouched/leaning poses request their matching stand animation and report her not
        // yet standing; walking/transition poses simply wait for their current animation.
        bool stoodUp = _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        if (stoodUp)
        {
            _state.Phase = MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire;
            _state.PhaseTimer = 16;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off admire body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffAdmire()
    {
        // Stand-up falls through here, so the freshly written 16 becomes 15
        // on the same AI call. The sign test accepts zero and expires only after underflow.
        if (--_state.PhaseTimer < 0)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.StretchingPhase2);
            _state.Phase = MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam;
            _state.PhaseTimer = 256;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off charge final beam body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffChargeFinalBeam()
    {
        if (--_state.PhaseTimer < 0)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.ChargingRainbowBeam);
            _state.Phase = MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off load baby tiles body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
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
            _state.Phase = MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam;
            _state.PhaseTimer = 256;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off fire final beam body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffFireFinalBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.PhaseTimer < 0)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.FiringRainbowBeam);
            _state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            requests.FinalBeamSoundQueued = true; // The final beam's sound.
            _state.Phase = MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase final rainbow beam holding body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunSecondPhaseFinalRainbowBeamHolding()
    {
        // The final beam's holding step is empty. Mother Brain remains here
        // until the independently spawned Baby actor overwrites her body function
        // with the taken-aback step after latching onto her head.
        return false;
    }
}
