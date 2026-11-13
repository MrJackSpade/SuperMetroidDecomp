using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// The ordinary hurt-counter branch of
/// Samus's miscellaneous colours.
/// </summary>
/// <remarks>
/// The caller must first dispatch any nonzero super-special palette flag, just as the
/// game's suit colouring does. Runtime satisfies that contract by allowing the drained/rainbow
/// handler to return before this step. All remaining special palettes run earlier and
/// this step therefore has final priority over charge, Speed Booster, shinespark,
/// Crystal Flash, and X-ray colors during its first six calls.
/// </remarks>
public static class SamusHurtFlashPalette
{
    /// <summary>The hurt-flash count at which the flash ends and the count is cleared: its largest value.</summary>
    public const int EndingCall = 60;

    /// <summary>
    /// Advances one hurt-flash call, optionally showing the flash's or the restored suit's shade as the suit's colours
    /// (#1512) and publishing the exact recovery sound side effects.
    /// </summary>
    /// <param name="colours">Samus's colours, whose suit shade the flash may replace.</param>
    /// <param name="samus">Owner of the hurt counter, equipment palettes, movement/grapple state, sound queue, and deferred charging-audio latch.</param>
    /// <param name="controllerInput">Current held buttons; X permits the deferred charging-beam sound request.</param>
    /// <returns>An empty step marker; counter, palette, and audio outcomes are applied directly to the supplied state.</returns>
    public static SamusHurtFlashPaletteStepResult Update(
        SamusColours colours,
        SamusState samus,
        ControllerButton controllerInput)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        int counterBefore = samus.HurtFlash.Count;
        if (counterBefore == 0)
        {
            return new SamusHurtFlashPaletteStepResult();
        }


        // The hurt flash plays the impact sound exactly on call two. Cinematics and the
        // Mother-Brain drained knockback handler are the two exceptions; the latter is
        // identified by the same explicit phase/pose pair that replaces its handler.
        if (counterBefore == 2 &&
            !samus.LiquidPhysics.CinematicFunctionActive &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked &&
              samus.Pose == SamusPoseId.KnockbackLeftPose))
        {
            samus.LiquidPhysics.QueueMovementSound(SamusSound.SamusDamaged, priority: SoundRequestPriority.Normal);
        }

        // Only calls one through six write colors. Odd calls select the fixed hurt palette;
        // even calls restore either the intro palette or the equipment-selected suit. Once
        // counter seven is reached the existing colours deliberately remain untouched.
        if (counterBefore < 7)
        {
            if (SamusCadenceDefinitions.HurtColoursShown.IsSetIn(counterBefore))
            {
                colours.ShowSuit(SamusShades.Hurt(SamusHurtColorVariant.Hurt));
            }
            else if (samus.LiquidPhysics.CinematicFunctionActive)
            {
                colours.ShowSuit(SamusShades.Hurt(SamusHurtColorVariant.Intro));
            }
            else
            {
                SamusNormalSuitPalette.Load(colours, samus.EquippedItems);
            }
        }

        // The game stores the increment before either comparison. The counter starts at one or eight and is
        // cleared at sixty, so it never leaves 0 through 60.
        int counterAfter = samus.HurtFlash.Advance();

        if (counterAfter == 40)
            RecoverInterruptedSound(samus, controllerInput);

        if (counterAfter >= EndingCall)
        {
            samus.HurtFlash.Clear();
        }

        return new SamusHurtFlashPaletteStepResult();
    }

    /// <summary>
    /// Consumes the pending charging-sound resume in the same post-draw position as the game's post-draw check.
    /// </summary>
    /// <param name="samus">Owner of the resume-charging flag, which is cleared whenever set regardless of the held buttons.</param>
    /// <param name="controllerInput">Current held buttons; X must still be held to queue the resume-charging sound.</param>
    /// <returns>Whether the nonzero latch caused a resume-charging sound request; this is not confirmation of playback.</returns>
    public static bool ConsumeResumeChargingBeamSound(SamusState samus, ControllerButton controllerInput)
    {
        Ensure.NotNull(samus);
        if (!samus.ResumeChargingBeamSoundPending)
            return false;

        bool queued = (controllerInput & ControllerButton.X) != ControllerButton.None;
        if (queued)
        {
            samus.LiquidPhysics.QueueMovementSound(SamusSound.ResumeChargingBeam, priority: SoundRequestPriority.High);
        }

        samus.ResumeChargingBeamSoundPending = false;
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
                samus.LiquidPhysics.QueueMovementSound(sound, priority: SoundRequestPriority.High);
                return sound == SamusSound.ScrewAttack
                    ? SamusHurtFlashRecoveryAction.ScrewAttackSound
                    : sound == SamusSound.SpaceJump
                        ? SamusHurtFlashRecoveryAction.SpaceJumpSound
                        : SamusHurtFlashRecoveryAction.SpinJumpSound;
            }

            if (samus.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter) &&
                (controllerInput & ControllerButton.X) != ControllerButton.None)
            {
                samus.ResumeChargingBeamSoundPending = true;
                return SamusHurtFlashRecoveryAction.ResumeChargingBeamRequested;
            }

            return SamusHurtFlashRecoveryAction.None;
        }

        // The grapple-function comparison accepts firing, swinging, locked,
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

        samus.LiquidPhysics.QueueMovementSound(SamusSound.Grappling, priority: SoundRequestPriority.High);
        return SamusHurtFlashRecoveryAction.GrappleSound;
    }

}
