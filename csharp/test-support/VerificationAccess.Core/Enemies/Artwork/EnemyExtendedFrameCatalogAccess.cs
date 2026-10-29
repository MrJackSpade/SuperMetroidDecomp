using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="EnemyExtendedFrameCatalog"/> members production does not use.</summary>
internal static class EnemyExtendedFrameCatalogAccess
{
    /// <summary>Exposes <see cref="EnemyExtendedFrameCatalog"/> state that only verification reads.</summary>
    extension(EnemyExtendedFrameCatalog self)
    {
        /// <summary>Finds a frame's components, including the shared empty extended frame's.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
            EnemyExtendedFrames.TryGet(self, frame, out components);
    }
}
