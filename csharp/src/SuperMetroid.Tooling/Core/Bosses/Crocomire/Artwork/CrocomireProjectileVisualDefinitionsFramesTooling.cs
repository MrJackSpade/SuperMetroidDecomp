using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Frame lists and counts of <see cref="CrocomireProjectileVisualDefinitions"/> that only development tools read.</summary>
internal static class CrocomireProjectileVisualDefinitionsFramesTooling
{
    /// <summary>The members, read as members of <see cref="CrocomireProjectileVisualDefinitions"/>.</summary>
    extension(CrocomireProjectileVisualDefinitions)
    {
        /// <summary>Every projectile frame in installed-artwork order: mouth, bridge, spike, then shot.</summary>
        internal static IReadOnlyList<CrocomireProjectileFrame> ProjectileFrames =>
        [
            .. Enumerable.Range(0, CrocomireAnimations.MouthPhaseCount).Select(phase => new CrocomireProjectileFrame(CrocomireProjectileFrameKind.Mouth, phase)),
            new CrocomireProjectileFrame(CrocomireProjectileFrameKind.BridgeFragment, 0),
            new CrocomireProjectileFrame(CrocomireProjectileFrameKind.SpikeWallPiece, 0),
            .. Enumerable.Range(0, CrocomireAnimations.ShotPhaseCount).Select(phase => new CrocomireProjectileFrame(CrocomireProjectileFrameKind.Shot, phase)),
        ];

        /// <summary>Distinct projectile frames: six mouth, one bridge fragment, one spike piece and five shot frames.</summary>
        internal static int ProjectileFrameCount => 13;
    }
}
