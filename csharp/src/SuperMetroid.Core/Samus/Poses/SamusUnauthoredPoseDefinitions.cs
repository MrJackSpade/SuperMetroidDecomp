using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What Samus's three unauthored poses define (<see cref="UnauthoredPoseReads{TPose, TDefinition}"/>).
/// </summary>
internal static class SamusUnauthoredPoseDefinitions
{
    /// <summary>
    /// The three poses' definitions. The tall still pose faces right, places a held charge's flare at the neutral turning
    /// placement, has a vertical radius of 138 pixels, has no movement type, and falls back with no input to the rightward
    /// neutral jump transition; the forward standing pose faces neither way, places the flare at the muzzle aiming down
    /// facing left, has the front view's radius, moves as standing, and keeps its pose with no input; the forward still
    /// pose faces neither way, places the flare at the muzzle aiming down facing right, has a crouch's radius, has no
    /// movement type, and falls back with no input to landing aiming diagonally down facing left.
    /// </summary>
    internal static readonly UnauthoredPoseReads<SamusPoseId, SamusUnauthoredPoseDefinition> Poses = new(
        new Dictionary<SamusPoseId, SamusUnauthoredPoseDefinition>
        {
            [SamusPoseId.TallStillRightPose] = new(
                SamusFacingDirection.Right,
                SamusPoseAim.Unaimed(SamusFlareAim.WhileTurning(SamusTurningAim.Neutral)),
                VerticalRadius: 138,
                ProjectileYOffset: 194,
                Movement: null,
                NoInputPose: SamusPoseId.NeutralJumpTransitionRightPose),
            [SamusPoseId.ForwardStandingPose] = new(
                SamusFacingDirection.ForwardOrSpecial,
                SamusPoseAim.Unaimed(SamusFlareAim.AtMuzzle(SamusProjectileDirection.DownFacingLeft)),
                VerticalRadius: 24,
                ProjectileYOffset: 22,
                Movement: SamusMovementType.Standing,
                NoInputPose: SamusMovementDefinitions.Poses.RetainCurrentPoseFallback),
            [SamusPoseId.ForwardStillCrouchHeightPose] = new(
                SamusFacingDirection.ForwardOrSpecial,
                SamusPoseAim.Unaimed(SamusFlareAim.AtMuzzle(SamusProjectileDirection.DownFacingRight)),
                VerticalRadius: 16,
                ProjectileYOffset: 133,
                Movement: null,
                NoInputPose: SamusPoseId.LandingAimDiagonalDownLeftPose),
        });
}
