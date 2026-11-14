using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>Post-draw spin/charge sound handoff from the shoot check.</summary>
public static class SamusPostDrawAudio
{
    /// <summary>
    /// Uses the completed previous frame's movement-type snapshot, not the last pose
    /// initializer's. A spin can end through aiming, firing, landing or another
    /// transition; the shared post-draw check handles them without per-input patches.
    /// </summary>
    public static void Step(SamusState samus,
        SamusMovementType previousMovement, ControllerButton controllerInput)
    {
        SamusHurtFlashPalette.ConsumeResumeChargingBeamSound(samus, controllerInput);
        var speed = samus.HorizontalSpeed;
        if (speed.EchoSoundPlaying &&
            !speed.SpeedBoostCounter.IsAtFullStage)
        {
            speed.EchoSoundPlaying = false;
            samus.LiquidPhysics.QueueMovementSound(RoomSound.SpeedBoosterOrElevatorStop, priority: SoundRequestPriority.Essential);
        }
        if (previousMovement is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) ||
            SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            return;

        samus.LiquidPhysics.QueueMovementSound(SamusSound.SpinJumpEnd, priority: SoundRequestPriority.Essential);
        // Resume on the NEXT post-draw pass, after the spin-end sound. The runtime
        // supplies normalized controls, so X here is the configured Shoot action.
        if (samus.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter) &&
            (controllerInput & ControllerButton.X) != ControllerButton.None)
            samus.ResumeChargingBeamSoundPending = true;
    }
}
