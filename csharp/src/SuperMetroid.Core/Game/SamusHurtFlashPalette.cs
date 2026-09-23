using SuperMetroid.Core.Assets;
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
    /// <summary>
    /// Advances one native hurt-flash call, optionally replacing all sixteen colors of
    /// OBJ palette four and publishing the exact recovery sound side effects.
    /// </summary>
    public static SamusHurtFlashPaletteStepResult Update(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus,
        ushort controllerInput,
        SamusHurtColorCatalog? presentationColors = null)
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
                LoadPresentationOrNative(SamusHurtColorVariant.Hurt,
                    SamusPaletteRomData.HurtFlash.Colors);
                action = SamusHurtFlashPaletteAction.HurtFlash;
                paletteAddress = SamusPaletteRomData.HurtFlash.Colors;
            }
            else if (samus.LiquidPhysics.CinematicFunctionActive)
            {
                LoadPresentationOrNative(SamusHurtColorVariant.Intro,
                    SamusPaletteRomData.HurtFlash.IntroColors);
                action = SamusHurtFlashPaletteAction.IntroRestore;
                paletteAddress = SamusPaletteRomData.HurtFlash.IntroColors;
            }
            else
            {
                ushort suitOffset = samus.EquippedItems.GetSuitPaletteTableOffset();
                ushort palettePointer =
                    SamusPaletteRomData.Common.NormalSuitPalettePointer(suitOffset);
                paletteAddress = SamusPaletteRomData.Banks.Palette | palettePointer;
                cgram.LoadFromBus(
                    bus,
                    paletteAddress.Value,
                    colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
                    destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);
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

        void LoadPresentationOrNative(SamusHurtColorVariant variant, int nativeAddress)
        {
            if (presentationColors is null)
            {
                cgram.LoadFromBus(bus, nativeAddress,
                    colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
                    destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);
                return;
            }
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index,
                    presentationColors.Resolve(variant, index));
        }
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
            if (SamusSpinSoundCommand.Select(bus, samus) is { } sound)
            {
                samus.LiquidPhysics.QueueMovementSound(sound, maximumQueued: 9);
                return sound == SoundEffectLibrary1Sounds.ScrewAttack
                    ? SamusHurtFlashRecoveryAction.ScrewAttackSound
                    : sound == SoundEffectLibrary1Sounds.SpaceJump
                        ? SamusHurtFlashRecoveryAction.SpaceJumpSound
                        : SamusHurtFlashRecoveryAction.SpinJumpSound;
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
