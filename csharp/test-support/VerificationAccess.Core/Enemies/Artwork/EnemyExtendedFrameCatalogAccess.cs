using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="EnemyExtendedFrameCatalog"/> members production does not use.</summary>
internal static class EnemyExtendedFrameCatalogAccess
{
    /// <summary>Exposes <see cref="EnemyExtendedFrameCatalog"/> state that only verification reads.</summary>
    extension(EnemyExtendedFrameCatalog self)
    {
        /// <summary>Finds a frame's components, including the empty frame's (none).</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
            EnemyExtendedFrames.Components(self, EnemyExtendedFrames.TryResolve(self, frame, out EnemyFrameId? stored),
                stored, out components);
    }
}
