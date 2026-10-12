using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Selects the current spin sound for cartridge Samus command $1C ($90:F41E).</summary>
internal static class SamusSpinSoundCommand
{
    /// <summary>Selects.</summary>
    public static SoundEffectId? Select(ISnesAddressSpace bus, SamusState samus)
    {
        var movement = samus.PoseMetadata.ReadMovementType(bus);
        if (movement == SamusMovementType.WallJumping)
            return samus.AnimationFrame >= SamusSpinSoundFrames.ScrewAttack
                ? SoundEffectLibrary1Sounds.ScrewAttack
                : samus.AnimationFrame >= SamusSpinSoundFrames.SpaceJump
                    ? SoundEffectLibrary1Sounds.SpaceJump : SoundEffectLibrary1Sounds.SpinJump;
        if (movement != SamusMovementType.SpinJumping)
            return null;
        return samus.Pose is SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose
            ? SoundEffectLibrary1Sounds.ScrewAttack
            : SamusPoseMetadata.IsSpaceJumpPose(samus.Pose)
                ? SoundEffectLibrary1Sounds.SpaceJump : SoundEffectLibrary1Sounds.SpinJump;
    }
}
