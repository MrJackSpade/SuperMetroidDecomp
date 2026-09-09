using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>Post-draw spin/charge handoff from Samus_ShootCheck at $90:F576.</summary>
public static class SamusPostDrawAudio
{
    /// <summary>
    /// Uses the completed previous frame's $0A11 movement snapshot, not the last pose
    /// initializer's $0A23. A spin can end through aiming, firing, landing or another
    /// transition; the shared post-draw check handles them without per-input patches.
    /// </summary>
    public static void Step(ISnesAddressSpace bus, SamusState samus,
        SamusMovementType previousMovement, ushort controllerInput)
    {
        // The signed latch takes native's direct branch to rearming positive one. It
        // must not play a charge sound or run the spin-stop test on this frame.
        if (unchecked((short)samus.ResumeChargingBeamSoundFlag) < 0)
        {
            samus.ResumeChargingBeamSoundFlag = 1;
            return;
        }

        SamusHurtFlashPalette.ConsumeResumeChargingBeamSound(samus, controllerInput);
        if (previousMovement is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) ||
            samus.ReadMovementType(bus) is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            return;

        samus.LiquidPhysics.QueueMovementSound(SoundEffectLibrary1Sounds.StopSpinJump, maximumQueued: 15);
        // Resume on the NEXT post-draw pass, after the spin-stop command. The runtime
        // supplies normalized controls, so X here is the configured Shoot action.
        if (samus.ProjectileFlareCounter >= SamusProjectileRomData.Beams.ChargeSoundStartCounter &&
            (controllerInput & (ushort)SnesButton.X) != 0)
            samus.ResumeChargingBeamSoundFlag = 1;
    }
}
