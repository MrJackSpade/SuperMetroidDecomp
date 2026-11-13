using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>Selects the spin sound Samus's current spin plays.</summary>
internal static class SamusSpinSound
{
    /// <summary>Selects spin, space-jump, or screw-attack audio from movement, pose, and wall-jump frame; returns null outside spin movement.</summary>
    public static SoundEffectId? Select(SamusState samus)
    {
        var movement = SamusPoseMetadata.ReadMovementType(samus);
        if (movement == SamusMovementType.WallJumping)
            return samus.AnimationFrame >= SamusSpinSoundFrames.ScrewAttack
                ? SamusSound.ScrewAttack
                : samus.AnimationFrame >= SamusSpinSoundFrames.SpaceJump
                    ? SamusSound.SpaceJump : SamusSound.SpinJump;
        if (movement != SamusMovementType.SpinJumping)
            return null;
        return samus.Pose is SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose
            ? SamusSound.ScrewAttack
            : SamusPoseMetadata.IsSpaceJumpPose(samus.Pose)
                ? SamusSound.SpaceJump : SamusSound.SpinJump;
    }
}
