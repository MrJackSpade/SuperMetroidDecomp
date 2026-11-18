namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The projectile origin height of each pose.</summary>
internal static class SamusPoseProjectileOrigins
{
    /// <summary>
    /// How far above Samus's centre the projectiles and Grapple of <paramref name="pose"/> start, in pixels. The drained
    /// poses' origin is four pixels below the centre; they cannot shoot or fire the Grapple, so nothing fired from them
    /// reads it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static int ReadYOffset(SamusPoseId pose) => SamusPoseTraits.Of(pose).ProjectileHeight;
}
