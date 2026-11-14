using static SuperMetroid.Core.Samus.Poses.SamusPoseProjectileOriginDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The projectile origin height of each pose.</summary>
internal static class SamusPoseProjectileOrigins
{
    /// <summary>
    /// How far above Samus's centre the projectiles and Grapple of <paramref name="pose"/> start, in pixels, 0 through
    /// 255: its authored height, or what an unauthored pose reads
    /// (<see cref="SamusUnauthoredPoseDefinitions"/>). The drained poses' height is 252: their correction lowers the
    /// origin by four, which the beam and Grapple origins take as a height of 252 rather than four below the centre.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static int ReadYOffset(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored) ? unauthored.ProjectileYOffset
        : YOffsets.TryGetValue(pose, out int offset) ? offset
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
