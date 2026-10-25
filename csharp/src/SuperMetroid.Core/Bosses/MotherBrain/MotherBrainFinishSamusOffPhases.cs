using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
internal sealed class MotherBrainFinishSamusOffPhases
{
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFinishSamusOffPhases(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>Rainbow-beam palette animation index reset immediately before the final shot.</summary>
    public int RainbowBeamPaletteAnimationIndex { get; internal set; }

    /// <summary>Applies Gravity/Varia damage reduction and the native extra scaling used only by the $50 beam strike.</summary>
    /// <remarks>The nominal damage is 80 or 160, so the scaled threshold never exceeds 340.</remarks>
    private static int CalculateFinishOffHealthThreshold(SamusState samus, int nominalDamage)
    {
        // `$A0:A45E` gives Gravity Suit priority and divides damage by four. Otherwise
        // Varia's bit zero divides by two; Power Suit leaves it untouched. `$A9:BD4C`
        // then multiplies the divided `$50` result by four, while `$BD68` deliberately
        // does not multiply the divided `$A0` result. The caller selects that distinction.
        int divided = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? nominalDamage >> 2
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? nominalDamage >> 1
                : nominalDamage;

        return nominalDamage == 80
            ? divided * 4 + 20
            : divided + 20;
    }

    /// <summary>Selects the next Baby Metroid page, rejecting calls after its last page has shown.</summary>
    private MotherBrainSpritePage NextBabyMetroidPage()
    {
        int index = _state.BabyMetroidPagesShown;
        if (index >= MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.BabyMetroid))
            throw new InvalidOperationException("Every Baby Metroid page has already shown.");

        _state.BabyMetroidPagesShown++;
        return MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.BabyMetroid, index);
    }

    /// <summary>
    /// Runs the second phase finish samus off body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOff(SamusState samus, RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // `$A9:BD45-$BD54` asks whether one more ordinary phase-two hit could put
        // Samus at the encounter's intended low-energy floor. The first nominal
        // damage is `$50`, suit-divided, multiplied by four, then padded by twenty.
        // The native BPL comparison treats equality as done and changes the body
        // function without falling into the stand-up handler on this call.
        if (CalculateFinishOffHealthThreshold(samus, 80) >= samus.Health)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp;
            return false;
        }

        // Above the floor, 4000/4096 low-twelve-bit RNG values do nothing. Every
        // 32nd enemy frame that idle route may ask the posture helper to alternate
        // between standing and leaning down. This is visible body bytecode, not a
        // direct pose assignment, so retain the separate instruction-stage delay.
        if (frame.RandomNumberSeed.LowBits(12) < 4000)
        {
            if ((frame.EnemyFrameCounter & 31) == 0)
                requests.BodyPostureRequested = _state.MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
            return false;
        }

        // A weaker `$A0` nominal-hit threshold biases wounded Samus toward onion
        // rings. Only when Samus is above it does the top 16/4096 RNG band choose a
        // bomb; all remaining admitted values still choose two onion rings.
        if (CalculateFinishOffHealthThreshold(samus, 160) >= samus.Health ||
            frame.RandomNumberSeed.LowBits(12) < 4080)
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
    /// Runs the second phase finish samus off stand up body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffStandUp(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$A9:C670` reports carry only if pose was already zero at call entry.
        // Crouched/leaning poses request their matching stand animation and return
        // clear; walking/transition poses simply wait for their current bytecode.
        bool stoodUp = _state.MakeBodyStandUp(out bool postureRequested);
        requests.BodyPostureRequested = postureRequested;
        if (stoodUp)
        {
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire;
            _state.FunctionTimer = WrappingCountdown.Start(16);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off admire body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffAdmire()
    {
        // Stand-up falls through here, so the freshly written `$10` becomes `$0F`
        // on the same AI call. BPL accepts zero and expires only after underflow.
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.StretchingPhase2);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam;
            _state.FunctionTimer = WrappingCountdown.Start(256);
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off charge final beam body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffChargeFinalBeam()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.ChargingRainbowBeam);
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off load baby tiles body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffLoadBabyTiles(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$A9:C5BE` processes exactly one seven-byte table entry per call. The
        // fourth entry sees the following zero terminator, clears its saved pointer,
        // and returns carry set on that same call.
        requests.SpritePage = NextBabyMetroidPage();
        if (_state.BabyMetroidPagesShown == MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.BabyMetroid))
        {
            _state.RetractHead();
            _state.BabyMetroidSpawned = true;
            requests.BabySpawnRequested = true;
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam;
            _state.FunctionTimer = WrappingCountdown.Start(256);
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase finish samus off fire final beam body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunSecondPhaseFinishSamusOffFireFinalBeam(MotherBrainRainbowBeamStepRequests requests)
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            RainbowBeamPaletteAnimationIndex = 0;
            _state.SetHeadProgram(MotherBrainHeadProgram.FiringRainbowBeam);
            _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            requests.FinalBeamSoundQueued = true; // Sound library two, effect `$71`.
            _state.Phase = MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding;
        }
        return false;
    }

    /// <summary>
    /// Runs the second phase final rainbow beam holding body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunSecondPhaseFinalRainbowBeamHolding()
    {
        // `$A9:BE14` stores the address of its own RTS. Mother Brain remains here
        // until the independently spawned Baby actor overwrites her body function
        // with `$BE38` after latching onto her head.
        return false;
    }
}
