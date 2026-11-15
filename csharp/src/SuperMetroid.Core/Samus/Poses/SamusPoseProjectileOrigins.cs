using static SuperMetroid.Core.Samus.Poses.SamusPoseProjectileOriginDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The projectile origin height of each pose.</summary>
internal static class SamusPoseProjectileOrigins
{
    /// <summary>
    /// How far above Samus's centre the projectiles and Grapple of <paramref name="pose"/> start, in pixels: its authored
    /// height, or what an unauthored pose reads (<see cref="SamusUnauthoredPoseDefinitions"/>). The drained poses' origin
    /// is four pixels below the centre. The original read their correction as a byte, a height of 252; the drained poses
    /// cannot shoot or fire the Grapple, so nothing fired from them reads it, and that representation is gone (#1928).
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static int ReadYOffset(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored) ? unauthored.ProjectileYOffset
        : YOffsets.TryGetValue(pose, out int offset) ? offset
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
