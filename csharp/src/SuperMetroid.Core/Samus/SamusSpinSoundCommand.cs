using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>Selects the current spin sound for cartridge Samus command $1C ($90:F41E).</summary>
internal static class SamusSpinSoundCommand
{
    /// <summary>Selects spin, space-jump, or screw-attack audio from movement, pose, and wall-jump frame; returns null outside spin movement.</summary>
    public static SoundEffectId? Select(SamusState samus)
    {
        var movement = SamusPoseMetadata.ReadMovementType(samus);
        if (movement == SamusMovementType.WallJumping)
            return samus.AnimationFrame >= SamusSpinSoundFrames.ScrewAttack
                ? Library1Sound.ScrewAttack
                : samus.AnimationFrame >= SamusSpinSoundFrames.SpaceJump
                    ? Library1Sound.SpaceJump : Library1Sound.SpinJump;
        if (movement != SamusMovementType.SpinJumping)
            return null;
        return samus.Pose is SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose
            ? Library1Sound.ScrewAttack
            : SamusPoseMetadata.IsSpaceJumpPose(samus.Pose)
                ? Library1Sound.SpaceJump : Library1Sound.SpinJump;
    }
}
