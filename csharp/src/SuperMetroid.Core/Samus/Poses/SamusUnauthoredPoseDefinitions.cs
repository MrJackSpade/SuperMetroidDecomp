using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What the three pose numbers past Samus's authored poses define (<see cref="UnauthoredPoseReads{TPose, TDefinition}"/>).
/// </summary>
internal static class SamusUnauthoredPoseDefinitions
{
    /// <summary>
    /// The three poses' definitions. The first faces right, places a held charge's flare at the neutral turning placement,
    /// has a vertical radius of 138 pixels, reads a movement beyond every movement type, and falls back with no input to
    /// the rightward neutral jump transition; the second faces neither way, places the flare at the muzzle aiming down
    /// facing left, has the front view's radius, moves as standing, and keeps its pose with no input; the third
    /// faces neither way, places the flare at the muzzle aiming down facing right, has a crouch's radius, reads a movement
    /// beyond every movement type, and falls back with no input to landing aiming diagonally down facing left.
    /// </summary>
    internal static readonly UnauthoredPoseReads<SamusPoseId, SamusUnauthoredPoseDefinition> Poses = new(
        new Dictionary<SamusPoseId, SamusUnauthoredPoseDefinition>
        {
            [SamusPoseId.FirstUnauthoredPose] = new(
                SamusFacingDirection.Right,
                SamusPoseAim.Unaimed(SamusFlareAim.WhileTurning(SamusTurningAim.Neutral)),
                VerticalRadius: 138,
                ProjectileYOffset: 194,
                Movement: null,
                NoInputPose: SamusPoseId.NeutralJumpTransitionRightPose),
            [SamusPoseId.SecondUnauthoredPose] = new(
                SamusFacingDirection.ForwardOrSpecial,
                SamusPoseAim.Unaimed(SamusFlareAim.AtMuzzle(SamusProjectileDirection.DownFacingLeft)),
                VerticalRadius: 24,
                ProjectileYOffset: 22,
                Movement: SamusMovementType.Standing,
                NoInputPose: SamusMovementDefinitions.Poses.RetainCurrentPoseFallback),
            [SamusPoseId.LastUnauthoredPose] = new(
                SamusFacingDirection.ForwardOrSpecial,
                SamusPoseAim.Unaimed(SamusFlareAim.AtMuzzle(SamusProjectileDirection.DownFacingRight)),
                VerticalRadius: 16,
                ProjectileYOffset: 133,
                Movement: null,
                NoInputPose: SamusPoseId.LandingAimDiagonalDownLeftPose),
        });
}
