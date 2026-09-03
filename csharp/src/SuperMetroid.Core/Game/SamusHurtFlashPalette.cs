using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal translation of the ordinary hurt-counter branch in
/// <c>HandleMiscSamusPalette</c> at <c>$91:D8AA-$91:D953</c>.
/// </summary>
/// <remarks>
/// The caller must first dispatch any nonzero super-special palette flag, just as native
/// <c>$91:D8A5</c> does. Runtime satisfies that contract by allowing the drained/rainbow
/// handler to return before this routine. All remaining special palettes run earlier and
/// this routine therefore has final priority over charge, Speed Booster, shinespark,
/// Crystal Flash, and X-ray colors during its first six calls.
/// </remarks>
public static class SamusHurtFlashPalette
{
    private const int NormalSuitPalettePointerTable = 0x91d727;
    private const int HurtFlashPalette = 0x9ba380;
    private const int IntroSamusPalette = 0x9ba3a0;
    private const int SamusPaletteCgramIndex = 192;

    /// <summary>
    /// Advances one native hurt-flash call, optionally replacing all sixteen colors of
    /// OBJ palette four and publishing the exact recovery sound side effects.
    /// </summary>
    public static SamusHurtFlashPaletteStepResult Update(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus,
        ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(samus);

        ushort counterBefore = samus.HurtFlashCounter;
        if (counterBefore == 0)
        {
            return new SamusHurtFlashPaletteStepResult(
                SamusHurtFlashPaletteAction.Inactive,
                CounterBefore: 0,
                CounterAfter: 0);
        }

        bool hurtSoundQueued = false;
        SamusHurtFlashPaletteAction action = SamusHurtFlashPaletteAction.NoPaletteChange;
        int? paletteAddress = null;

        // `$91:D8B3-$D8D4` plays the impact sound exactly on call two. Cinematics and the
        // Mother-Brain drained `$54` handler are the two native exceptions; the latter is
        // identified by the same explicit phase/pose pair that replaces its handler word.
        if (counterBefore == 2 &&
            !samus.LiquidPhysics.CinematicFunctionActive &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked &&
              samus.Pose == SamusPoseIds.KnockbackLeftPose))
        {
            samus.LiquidPhysics.QueueMovementSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x35), maximumQueued: 6);
            hurtSoundQueued = true;
        }

        // Only calls one through six write colors. Odd calls select the fixed hurt palette;
        // even calls restore either the intro palette or the equipment-selected suit. Once
        // counter seven is reached the existing CGRAM values deliberately remain untouched.
        if (unchecked((short)(counterBefore - 7)) < 0)
        {
            if ((counterBefore & 1) != 0)
            {
                cgram.LoadFromBus(
                    bus, HurtFlashPalette, colorCount: 16, destinationIndex: SamusPaletteCgramIndex);
                action = SamusHurtFlashPaletteAction.HurtFlash;
                paletteAddress = HurtFlashPalette;
            }
            else if (samus.LiquidPhysics.CinematicFunctionActive)
            {
                cgram.LoadFromBus(
                    bus, IntroSamusPalette, colorCount: 16, destinationIndex: SamusPaletteCgramIndex);
                action = SamusHurtFlashPaletteAction.IntroRestore;
                paletteAddress = IntroSamusPalette;
            }
            else
            {
                ushort suitOffset = samus.EquippedItems.GetSuitPaletteTableOffset();
                ushort palettePointer = ReadWord(
                    bus, NormalSuitPalettePointerTable + suitOffset);
                paletteAddress = 0x9b0000 | palettePointer;
                cgram.LoadFromBus(
                    bus, paletteAddress.Value, colorCount: 16,
                    destinationIndex: SamusPaletteCgramIndex);
                action = SamusHurtFlashPaletteAction.NormalSuitRestore;
            }
        }

        // Native stores the increment before either comparison. The signed branch against
        // sixty is preserved rather than simplified to an unsigned host range, retaining
        // the exact wrap behavior for manually corrupted/debugger-written counters.
        ushort counterAfter = unchecked((ushort)(counterBefore + 1));
        samus.HurtFlashCounter = counterAfter;

        SamusHurtFlashRecoveryAction recovery = SamusHurtFlashRecoveryAction.None;
        if (counterAfter == 40)
            recovery = RecoverInterruptedSound(bus, samus, controllerInput);

        if (unchecked((short)(counterAfter - 60)) >= 0)
        {
            samus.HurtFlashCounter = 0;
            counterAfter = 0;
        }

        return new SamusHurtFlashPaletteStepResult(
            action,
            counterBefore,
            counterAfter,
            paletteAddress,
            hurtSoundQueued,
            recovery);
    }

    /// <summary>
    /// Consumes the charging-audio latch in the same post-draw position as
    /// <c>$90:F576-$90:F58E</c>. The only translated producer writes positive one, but the
    /// property retains the native word so later power-bomb and door handlers can share it.
    /// </summary>
    public static bool ConsumeResumeChargingBeamSound(SamusState samus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (samus.ResumeChargingBeamSoundFlag == 0)
            return false;

        bool queued = (controllerInput & (ushort)SnesButton.X) != 0;
        if (queued)
        {
            samus.LiquidPhysics.QueueMovementSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x41), maximumQueued: 9);
        }

        samus.ResumeChargingBeamSoundFlag = 0;
        return queued;
    }

    private static SamusHurtFlashRecoveryAction RecoverInterruptedSound(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput)
    {
        if (samus.Grapple.Phase == GrapplePhase.Inactive)
        {
            SamusMovementType movementType = samus.ReadMovementType(bus);
            if (movementType is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // Command `$1C` chooses from current pose for a normal spin, but from
                // animation-frame ranges for wall jumping. Those are literal `$90:F41E`
                // thresholds, including frame 23 as the first Screw Attack frame.
                byte sound = movementType == SamusMovementType.WallJumping
                    ? samus.AnimationFrame >= 23 ? (byte)0x33 :
                      samus.AnimationFrame >= 13 ? (byte)0x3e : (byte)0x31
                    : samus.Pose is SamusPoseIds.ScrewAttackRightPose or SamusPoseIds.ScrewAttackLeftPose
                        ? (byte)0x33
                        : SamusState.IsSpaceJumpPose(samus.Pose) ? (byte)0x3e : (byte)0x31;
                samus.LiquidPhysics.QueueMovementSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, sound), maximumQueued: 9);
                return sound switch
                {
                    0x33 => SamusHurtFlashRecoveryAction.ScrewAttackSound,
                    0x3e => SamusHurtFlashRecoveryAction.SpaceJumpSound,
                    _ => SamusHurtFlashRecoveryAction.SpinJumpSound,
                };
            }

            if (samus.ProjectileFlareCounter >= 0x10 &&
                (controllerInput & (ushort)SnesButton.X) != 0)
            {
                samus.ResumeChargingBeamSoundFlag = 1;
                return SamusHurtFlashRecoveryAction.ResumeChargingBeamRequested;
            }

            return SamusHurtFlashRecoveryAction.None;
        }

        // The address comparison against `$9B:C856` accepts firing, swinging, locked,
        // wall-grab, and wall-grab-release functions. Cancel and all later teardown/drop
        // handlers intentionally leave grapple audio silent.
        bool grappleSoundActive = samus.Grapple.Phase is
            GrapplePhase.Firing or
            GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or
            GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease;
        if (!grappleSoundActive)
            return SamusHurtFlashRecoveryAction.None;

        samus.LiquidPhysics.QueueMovementSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x06), maximumQueued: 9);
        return SamusHurtFlashRecoveryAction.GrappleSound;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Palette write (or intentional non-write) performed by one hurt-counter call.</summary>
public enum SamusHurtFlashPaletteAction : byte
{
    Inactive,
    HurtFlash,
    NormalSuitRestore,
    IntroRestore,
    NoPaletteChange,
}

/// <summary>Counter-forty audio recovery selected by native movement/grapple state.</summary>
public enum SamusHurtFlashRecoveryAction : byte
{
    None,
    SpinJumpSound,
    SpaceJumpSound,
    ScrewAttackSound,
    GrappleSound,
    ResumeChargingBeamRequested,
}

/// <summary>Immutable debugger witness for one call of the hurt palette handler.</summary>
public readonly record struct SamusHurtFlashPaletteStepResult(
    SamusHurtFlashPaletteAction Action,
    ushort CounterBefore,
    ushort CounterAfter,
    int? PaletteAddress = null,
    bool HurtSoundQueued = false,
    SamusHurtFlashRecoveryAction Recovery = SamusHurtFlashRecoveryAction.None);
