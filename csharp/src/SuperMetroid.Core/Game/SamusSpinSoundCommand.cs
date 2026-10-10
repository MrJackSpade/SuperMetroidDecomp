using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Selects the current spin sound for cartridge Samus command $1C ($90:F41E).</summary>
internal static class SamusSpinSoundCommand
{
    /// <summary>
    /// Chooses the spin, Space Jump, or Screw Attack sound for spin-jump and wall-jump
    /// movement. Wall jumps use animation-frame thresholds, while spin jumps use Samus's pose.
    /// </summary>
    /// <param name="bus">Address space used to resolve Samus's current movement type.</param>
    /// <param name="samus">Samus state supplying the movement type and animation or pose used to classify the sound.</param>
    /// <returns>The selected library-one sound, or <see langword="null"/> when Samus is not spin-jumping or wall-jumping.</returns>
    public static SoundEffectId? Select(ISnesAddressSpace bus, SamusState samus)
    {
        var movement = samus.ReadMovementType(bus);
        if (movement == SamusMovementType.WallJumping)
            return samus.AnimationFrame >= SamusSpinSoundFrames.ScrewAttack
                ? SoundEffectLibrary1Sounds.ScrewAttack
                : samus.AnimationFrame >= SamusSpinSoundFrames.SpaceJump
                    ? SoundEffectLibrary1Sounds.SpaceJump : SoundEffectLibrary1Sounds.SpinJump;
        if (movement != SamusMovementType.SpinJumping)
            return null;
        return samus.Pose is SamusPoseIds.ScrewAttackRightPose or SamusPoseIds.ScrewAttackLeftPose
            ? SoundEffectLibrary1Sounds.ScrewAttack
            : SamusState.IsSpaceJumpPose(samus.Pose)
                ? SoundEffectLibrary1Sounds.SpaceJump : SoundEffectLibrary1Sounds.SpinJump;
    }
}
