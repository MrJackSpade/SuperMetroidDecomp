using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="EnemyArtworkCatalogsTooling"/> members production does not use.</summary>
internal static class EnemyArtworkCatalogsToolingAccess
{
    /// <summary>Exposes <see cref="EnemyArtworkCatalogsTooling"/> state that only verification reads.</summary>
    extension(EnemyExtendedFrameCatalog self)
    {
        /// <summary>The components of the frame stored at <paramref name="pointer"/> in <paramref name="bank"/>.</summary>
        internal bool TryGet(byte bank, ushort pointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
        {
            if (EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, pointer) is { } frame)
                return self.TryGet(frame, out components);
            components = default;
            return false;
        }
    }
}
