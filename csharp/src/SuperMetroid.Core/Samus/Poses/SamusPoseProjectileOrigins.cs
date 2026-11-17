
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The projectile origin height of each pose.</summary>
internal static class SamusPoseProjectileOrigins
{
    /// <summary>
    /// How far above Samus's centre the projectiles and Grapple of <paramref name="pose"/> start, in pixels: its authored
    /// height, or what an unauthored pose reads (<see cref="SamusUnauthoredPoseDefinitions"/>). The drained poses' origin
    /// is four pixels below the centre; they cannot shoot or fire the Grapple, so nothing fired from them reads it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static int ReadYOffset(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored) ? unauthored.ProjectileYOffset
        : SamusPoseTraits.Authored(pose).ProjectileHeight;
}
