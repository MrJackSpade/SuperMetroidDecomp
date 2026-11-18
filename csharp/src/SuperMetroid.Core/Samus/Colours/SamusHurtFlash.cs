using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The ordinary hurt flash among
/// Samus's miscellaneous colours.
/// </summary>
/// <remarks>
/// The drained or rainbow colouring runs first, just as the
/// game's suit colouring does. Runtime satisfies that contract by allowing the drained or rainbow
/// colouring to return before this step. All remaining special colourings run earlier and
/// this step therefore has final priority over charge, Speed Booster, shinespark,
/// Crystal Flash, and X-ray colors during its first six updates.
/// </remarks>
public static class SamusHurtFlash
{
    /// <summary>
    /// Advances one hurt-flash update, optionally showing the flash's or the restored suit's shade as the suit's colours
    /// (#1512) and publishing the exact recovery sound side effects.
    /// </summary>
    /// <param name="colours">Samus's colours, whose suit shade the flash may replace.</param>
    /// <param name="samus">Owner of the hurt counter, equipment shades, movement/grapple state, sound queue, and deferred charging-audio latch.</param>
    /// <param name="controllerInput">Current held actions; Shoot permits the deferred charging-beam sound request.</param>
    public static void Update(
        SamusColours colours,
        SamusState samus,
        GameplayAction controllerInput)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        int counterBefore = samus.HurtFlash.Count;
        if (counterBefore == 0)
        {
            return;
        }

// The hurt flash plays the impact sound on its impact update. Cinematics and the
        // Mother-Brain drained knockback are the two exceptions; the latter is
        // identified by the same explicit phase/pose pair that replaces its movement.
        if (counterBefore == SamusHurtFlashDefinitions.ImpactSoundUpdate &&
            samus.Presentation.Mode != SamusPresentationMode.Cinematic &&
            !(samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked &&
              samus.Pose == SamusPoseId.KnockbackLeftPose))
        {
            samus.AudioRequests.QueueSound(SamusSound.SamusDamaged, priority: SoundRequestPriority.Normal);
        }

        // Only the flash's coloured updates change colours: the first of each pair shows the hurt shade, the second
        // restores the intro shade or the equipped suit. Later updates leave the colours as they are.
        if (counterBefore <= SamusHurtFlashDefinitions.ColouredUpdates)
        {
            if (SamusCadenceDefinitions.HurtColoursShown.IsSetIn(counterBefore))
            {
                colours.ShowSuit(SamusShades.Hurt(SamusHurtColorVariant.Hurt));
            }
            else if (samus.Presentation.Mode == SamusPresentationMode.Cinematic)
            {
                colours.ShowSuit(SamusShades.Hurt(SamusHurtColorVariant.Intro));
            }
            else
            {
                SamusSuitShade.Show(colours, samus.EquippedItems);
            }
        }

        // The count advances before either comparison reads it. It starts at one or eight and clears at
        // sixty, so it never leaves 0 through 60.
        int counterAfter = samus.HurtFlash.Advance();

        if (counterAfter == SamusHurtFlashDefinitions.SoundRecoveryCount)
            RecoverInterruptedSound(samus, controllerInput);

        if (counterAfter >= SamusHurtFlashDefinitions.EndingUpdate)
        {
            samus.HurtFlash.Clear();
        }
    }

    /// <summary>
    /// Consumes the pending charging-sound resume in the same post-draw position as the game's post-draw check.
    /// </summary>
    /// <param name="samus">Owner of the resume-charging flag, which is cleared whenever set regardless of the held buttons.</param>
    /// <param name="controllerInput">Current held actions; Shoot must still be held to queue the resume-charging sound.</param>
    /// <returns>Whether a pending resume made a resume-charging sound request; this is not confirmation of playback.</returns>
    public static bool ConsumeResumeChargingBeamSound(SamusState samus, GameplayAction controllerInput)
    {
        Ensure.NotNull(samus);
        if (!samus.ResumeChargingBeamSoundPending)
            return false;

        bool queued = (controllerInput & GameplayAction.Shoot) != GameplayAction.None;
        if (queued)
        {
            samus.AudioRequests.QueueSound(SamusSound.ResumeChargingBeam, priority: SoundRequestPriority.High);
        }

        samus.ResumeChargingBeamSoundPending = false;
        return queued;
    }

    /// <summary>
    /// At hurt counter forty, restores the movement sound selected by Samus's current
    /// spin or active grapple state; a charged flare instead defers beam audio.
    /// </summary>
    private static void RecoverInterruptedSound(
        SamusState samus,
        GameplayAction controllerInput)
    {
        if (samus.Grapple.Phase == GrapplePhase.Inactive)
        {
            if (SamusSpinSound.Select(samus) is { } sound)
            {
                samus.AudioRequests.QueueSound(sound, priority: SoundRequestPriority.High);
                return;
            }

            if (samus.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter) &&
                (controllerInput & GameplayAction.Shoot) != GameplayAction.None)
            {
                samus.ResumeChargingBeamSoundPending = true;
            }

            return;
        }

        // The grapple-phase comparison accepts firing, swinging, locked,
        // wall-grab, and wall-grab-release phases. Cancel and all later teardown/drop
        // phases intentionally leave grapple audio silent.
        bool grappleSoundActive = samus.Grapple.Phase is
            GrapplePhase.Firing or
            GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or
            GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease;
        if (grappleSoundActive)
            samus.AudioRequests.QueueSound(SamusSound.Grappling, priority: SoundRequestPriority.High);
    }

}
