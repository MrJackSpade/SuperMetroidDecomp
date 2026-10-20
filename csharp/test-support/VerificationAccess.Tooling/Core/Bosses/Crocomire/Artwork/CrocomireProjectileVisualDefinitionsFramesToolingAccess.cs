namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Verification access to <see cref="CrocomireProjectileVisualDefinitionsFramesTooling"/> members production does not use.</summary>
internal static class CrocomireProjectileVisualDefinitionsFramesToolingAccess
{
    /// <summary>Exposes <see cref="CrocomireProjectileVisualDefinitionsFramesTooling"/> state that only verification reads.</summary>
    extension(CrocomireProjectileVisualDefinitions)
    {
        /// <summary>Distinct projectile frames: six mouth, one bridge fragment, one spike piece and five shot frames.</summary>
        internal static int ProjectileFrameCount => 13;
    }
}
