using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Frame lists and counts of <see cref="DraygonProjectileVisualDefinitions"/> that only development tools read.</summary>
internal static class DraygonProjectileVisualDefinitionsFramesTooling
{
    /// <summary>The members, read as members of <see cref="DraygonProjectileVisualDefinitions"/>.</summary>
    extension(DraygonProjectileVisualDefinitions)
    {
        /// <summary>Every frame in installed-artwork order: goop, burst, bloom, then flight.</summary>
        internal static IReadOnlyList<DraygonProjectileFrame> ProjectileFrames =>
        [
            .. DraygonProjectileVisualDefinitions.Frames(DraygonProjectileFrameKind.Goop, DraygonProjectileAnimations.GoopPhaseCount),
            .. DraygonProjectileVisualDefinitions.Frames(DraygonProjectileFrameKind.GoopShot, DraygonProjectileAnimations.GoopShotPhaseCount),
            .. DraygonProjectileVisualDefinitions.Frames(DraygonProjectileFrameKind.WallTurretBloom, DraygonProjectileAnimations.BloomPhaseCount),
            .. DraygonProjectileVisualDefinitions.Frames(DraygonProjectileFrameKind.WallTurretFlight, DraygonProjectileAnimations.FlightPhaseCount),
        ];

        /// <summary>The <paramref name="count"/> frames of <paramref name="kind"/>.</summary>
        private static IEnumerable<DraygonProjectileFrame> Frames(DraygonProjectileFrameKind kind, int count) =>
            Enumerable.Range(0, count).Select(phase => new DraygonProjectileFrame(kind, phase));
    }
}
