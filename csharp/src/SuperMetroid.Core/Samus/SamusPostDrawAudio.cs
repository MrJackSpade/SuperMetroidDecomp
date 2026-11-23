using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>Post-draw spin/charge sound handoff from the shoot check.</summary>
public static partial class SamusPostDrawAudio
{
    /// <summary>
    /// Uses the completed previous frame's movement-type snapshot, not the last pose
    /// setup's. A spin can end through aiming, firing, landing or another
    /// transition; the one post-draw check handles them all.
    /// </summary>
    public static void Step(SamusState samus,
        SamusMovementType previousMovement, GameplayAction controllerInput)
    {
        SamusHurtFlash.ResumeChargingBeamSound(samus, controllerInput);
        var speed = samus.HorizontalSpeed;
        if (speed.EchoSoundPlaying &&
            !speed.SpeedBoostCounter.IsAtFullStage)
        {
            speed.EchoSoundPlaying = false;
            samus.AudioRequests.QueueSound(RoomSoundStop.SpeedBoosterOrElevator, priority: SoundRequestPriority.Essential);
        }
        if (previousMovement is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) ||
            SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            return;

        samus.AudioRequests.QueueSound(SamusSoundStop.SpinJump, priority: SoundRequestPriority.Essential);
        // Resume on the NEXT post-draw pass, after the spin jump's stop, while Shoot is still
        // held.
        if (samus.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter) &&
            (controllerInput & GameplayAction.Shoot) != GameplayAction.None)
            samus.ChargingSound.Arm();
    }
}
