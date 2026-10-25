using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

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
    /// <summary>How many values the hurt-flash counter counts through before it returns to zero, as the cartridge's word does.</summary>
    private const int HurtFlashCounterPeriod = 65536;

    /// <summary>
    /// Advances one native hurt-flash call, optionally replacing all sixteen colors of
    /// OBJ palette four and publishing the exact recovery sound side effects.
    /// </summary>
    /// <param name="suit">Samus's suit colours, which the flash may replace.</param>
    /// <param name="samus">Owner of the hurt counter, equipment palettes, movement/grapple state, sound queue, and deferred charging-audio latch.</param>
    /// <param name="controllerInput">Current held SNES buttons; X permits the deferred charging-beam sound request.</param>
    /// <param name="presentationColors">Installed hurt/intro colors, required only when the selected flash call loads one of those palettes.</param>
    /// <returns>An empty step marker; counter, palette, and audio outcomes are applied directly to the supplied state.</returns>
    public static SamusHurtFlashPaletteStepResult Update(
        ColourRun suit,
        SamusState samus,
        ControllerButton controllerInput,
        SamusHurtColorCatalog? presentationColors = null)
    {
        Ensure.NotNull(samus);

        int counterBefore = samus.HurtFlashCounter;
        if (counterBefore == 0)
        {
            return new SamusHurtFlashPaletteStepResult();
        }


        // `$91:D8B3-$D8D4` plays the impact sound exactly on call two. Cinematics and the
        // Mother-Brain drained `$54` handler are the two native exceptions; the latter is
        // identified by the same explicit phase/pose pair that replaces its handler word.
        if (counterBefore == 2 &&
            !samus.LiquidPhysics.CinematicFunctionActive &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked &&
              samus.Pose == SamusPoseId.KnockbackLeftPose))
        {
            samus.LiquidPhysics.QueueMovementSound(Library1Sound.SamusDamaged, maximumQueued: SoundQueueLimit.Max6);
        }

        // Only calls one through six write colors. Odd calls select the fixed hurt palette;
        // even calls restore either the intro palette or the equipment-selected suit. Once
        // counter seven is reached the existing CGRAM values deliberately remain untouched.
        if (unchecked((short)(counterBefore - 7)) < 0)
        {
            if ((counterBefore & 1) != 0)
            {
                LoadPresentation(SamusHurtColorVariant.Hurt);
            }
            else if (samus.LiquidPhysics.CinematicFunctionActive)
            {
                LoadPresentation(SamusHurtColorVariant.Intro);
            }
            else
            {
                SamusNormalSuitPalette.Load(suit, samus.EquippedItems, samus.Presentation.SuitColors);
            }
        }

        // Native stores the increment before either comparison. The signed branch against
        // sixty is preserved rather than simplified to an unsigned host range, retaining
        // the exact wrap behavior for manually corrupted/debugger-written counters.
        int counterAfter = (counterBefore + 1) % HurtFlashCounterPeriod;
        samus.HurtFlashCounter = counterAfter;

        if (counterAfter == 40)
            RecoverInterruptedSound(samus, controllerInput);

        if (unchecked((short)(counterAfter - 60)) >= 0)
        {
            samus.HurtFlashCounter = 0;
        }

        return new SamusHurtFlashPaletteStepResult();

        void LoadPresentation(SamusHurtColorVariant variant)
        {
            if (presentationColors is null)
                throw new InvalidOperationException(
                    "Hurt-flash palette requires installed Samus hurt colors.");
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                suit.Write(index,
                    presentationColors.Resolve(variant, index));
        }
    }

    /// <summary>
    /// Consumes the charging-audio latch in the same post-draw position as
    /// <c>$90:F576-$90:F58E</c>. The only translated producer writes positive one, but the
    /// property retains the native word so later power-bomb and door handlers can share it.
    /// </summary>
    /// <param name="samus">Owner of the resume-charging flag, which is cleared whenever nonzero regardless of the held buttons.</param>
    /// <param name="controllerInput">Current held SNES buttons; X must still be held to queue library-one sound $41.</param>
    /// <returns>Whether the nonzero latch caused a resume-charging sound request; this is not confirmation of playback.</returns>
    public static bool ConsumeResumeChargingBeamSound(SamusState samus, ControllerButton controllerInput)
    {
        Ensure.NotNull(samus);
        if (samus.ResumeChargingBeamSound == ResumeChargingBeamSoundLatch.Clear)
            return false;

        bool queued = (controllerInput & ControllerButton.X) != ControllerButton.None;
        if (queued)
        {
            samus.LiquidPhysics.QueueMovementSound(Library1Sound.ResumeChargingBeam, maximumQueued: SoundQueueLimit.Max9);
        }

        samus.ResumeChargingBeamSound = ResumeChargingBeamSoundLatch.Clear;
        return queued;
    }

    /// <summary>
    /// At hurt counter forty, restores the movement sound selected by Samus's current
    /// spin or active grapple state; a charged flare instead defers beam audio.
    /// </summary>
    private static SamusHurtFlashRecoveryAction RecoverInterruptedSound(
        SamusState samus,
        ControllerButton controllerInput)
    {
        if (samus.Grapple.Phase == GrapplePhase.Inactive)
        {
            if (SamusSpinSoundCommand.Select(samus) is { } sound)
            {
                samus.LiquidPhysics.QueueMovementSound(sound, maximumQueued: SoundQueueLimit.Max9);
                return sound == Library1Sound.ScrewAttack
                    ? SamusHurtFlashRecoveryAction.ScrewAttackSound
                    : sound == Library1Sound.SpaceJump
                        ? SamusHurtFlashRecoveryAction.SpaceJumpSound
                        : SamusHurtFlashRecoveryAction.SpinJumpSound;
            }

            if (samus.ProjectileFlareCounter >= 16 &&
                (controllerInput & ControllerButton.X) != ControllerButton.None)
            {
                samus.ResumeChargingBeamSound = ResumeChargingBeamSoundLatch.ResumeNextPass;
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

        samus.LiquidPhysics.QueueMovementSound(Library1Sound.Grappling, maximumQueued: SoundQueueLimit.Max9);
        return SamusHurtFlashRecoveryAction.GrappleSound;
    }

}
