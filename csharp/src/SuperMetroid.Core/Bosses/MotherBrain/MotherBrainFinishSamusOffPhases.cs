using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
internal sealed class MotherBrainFinishSamusOffPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs and the brain's flags, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFinishSamusOffPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>Applies Gravity/Varia damage reduction and the extra scaling used only by the 80-damage beam strike.</summary>
    /// <remarks>The nominal damage is 80 or 160, so the scaled threshold never exceeds 340.</remarks>
    private static int CalculateFinishOffHealthThreshold(SamusState samus, int nominalDamage)
    {
        // Suit damage reduction gives Gravity Suit priority and divides damage by four. Otherwise
        // Varia's bit zero divides by two; Power Suit leaves it untouched. The low-energy check
        // then multiplies the divided 80 result by four, while the next check deliberately
        // does not multiply the divided 160 result. The caller selects that distinction.
        int divided = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? nominalDamage >> 2
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? nominalDamage >> 1
                : nominalDamage;

        return nominalDamage == 80
            ? divided * 4 + 20
            : divided + 20;
    }

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
        // The low-energy check asks whether one more ordinary phase-two hit could put
        // Samus at the encounter's intended low-energy floor. The first nominal
        // damage is 80, suit-divided, multiplied by four, then padded by twenty.
        // The sign test treats equality as done and changes the body
        // function without falling into the stand-up handler on this call.
        if (CalculateFinishOffHealthThreshold(samus, 80) >= samus.Health)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp;
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

        // A weaker 160 nominal-hit threshold biases wounded Samus toward onion
        // rings. Only when Samus is above it does the bomb's chance choose a
        // bomb; all remaining admitted values still choose two onion rings.
        if (CalculateFinishOffHealthThreshold(samus, 160) >= samus.Health ||
            !frame.RandomNumberSeed.Takes(MotherBrainRandomDefinitions.FinishOffBombs))
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.TwoOnionRingsPhase2);
        }
        else
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.BombPhase2);
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
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire;
            _state.FunctionTimer = 16;
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
        if (--_state.FunctionTimer < 0)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.StretchingPhase2);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam;
            _state.FunctionTimer = 256;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off charge final beam body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffChargeFinalBeam()
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.ChargingRainbowBeam);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffReadyBaby;
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
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam;
            _state.FunctionTimer = 256;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off fire final beam body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffFireFinalBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.FiringRainbowBeam);
            _state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            requests.FinalBeamSoundQueued = true; // The final beam's sound.
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding;
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
