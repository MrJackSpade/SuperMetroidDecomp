using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>Post-draw spin/charge handoff from Samus_ShootCheck at $90:F576.</summary>
public static class SamusPostDrawAudio
{
    /// <summary>
    /// Uses the completed previous frame's $0A11 movement snapshot, not the last pose
    /// initializer's $0A23. A spin can end through aiming, firing, landing or another
    /// transition; the shared post-draw check handles them without per-input patches.
    /// </summary>
    public static void Step(SamusState samus,
        SamusMovementType previousMovement, SnesButton controllerInput)
    {
        // The signed latch takes native's direct branch to rearming positive one. It
        // must not play a charge sound or run the spin-stop test on this frame.
        if (samus.ResumeChargingBeamSound == ResumeChargingBeamSoundLatch.SignedLatch)
        {
            samus.ResumeChargingBeamSound = ResumeChargingBeamSoundLatch.ResumeNextPass;
            return;
        }

        SamusHurtFlashPalette.ConsumeResumeChargingBeamSound(samus, controllerInput);
        var speed = samus.HorizontalSpeed;
        if (speed.EchoSoundFlag != 0 &&
            !speed.SpeedBoostCounter.HasActiveStageBit)
        {
            speed.EchoSoundFlag = 0;
            samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary3Sounds.StopSpeedBoosterEcho, maximumQueued: 15);
        }
        if (previousMovement is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) ||
            SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            return;

        samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.StopSpinJump, maximumQueued: 15);
        // Resume on the NEXT post-draw pass, after the spin-stop command. The runtime
        // supplies normalized controls, so X here is the configured Shoot action.
        if (samus.ProjectileFlareCounter >= SamusProjectileRomData.Beams.ChargeSoundStartCounter &&
            (controllerInput & SnesButton.X) != 0)
            samus.ResumeChargingBeamSound = ResumeChargingBeamSoundLatch.ResumeNextPass;
    }
}
