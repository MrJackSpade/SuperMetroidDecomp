using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What the three pose numbers past Samus's authored poses define (<see cref="UnauthoredPoseReads{TPose, TDefinition}"/>).
/// </summary>
internal static class SamusUnauthoredPoseDefinitions
{
    /// <summary>
    /// The three poses' definitions. The first faces right, places a held charge's flare at the neutral turning placement,
    /// and has a vertical radius of 138 pixels; the second faces neither way, places the flare at the muzzle aiming down facing
    /// left, and has the front view's radius; the third faces neither way, places the flare at the muzzle aiming down
    /// facing right, and has a crouch's radius.
    /// </summary>
    internal static readonly UnauthoredPoseReads<SamusPoseId, SamusUnauthoredPoseDefinition> Poses = new(
        new Dictionary<SamusPoseId, SamusUnauthoredPoseDefinition>
        {
            [SamusPoseId.AdjacentCodePoseFD] = new(
                SamusFacingDirection.Right,
                SamusPoseAim.Unaimed(SamusFlareAim.WhileTurning(SamusTurningAim.Neutral)),
                VerticalRadius: 138,
                ProjectileYOffset: 194),
            [SamusPoseId.AdjacentCodePoseFE] = new(
                SamusFacingDirection.ForwardOrSpecial,
                SamusPoseAim.Unaimed(SamusFlareAim.AtMuzzle(SamusProjectileDirection.DownFacingLeft)),
                VerticalRadius: 24,
                ProjectileYOffset: 22),
            [SamusPoseId.AdjacentCodePoseFF] = new(
                SamusFacingDirection.ForwardOrSpecial,
                SamusPoseAim.Unaimed(SamusFlareAim.AtMuzzle(SamusProjectileDirection.DownFacingRight)),
                VerticalRadius: 16,
                ProjectileYOffset: 133),
        });
}
