using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Development-tool views of the installed enemy artwork catalogs by cartridge address, for oracles that start from the
/// word the cartridge stores a frame at; never linked by player hosts.
/// </summary>
internal static class EnemyArtworkCatalogsTooling
{
    /// <summary>Ordinary compositions by cartridge address.</summary>
    extension(EnemySpritemapCatalog self)
    {
        /// <summary>The composition displayed for the frame stored at <paramref name="pointer"/> in <paramref name="bank"/>.</summary>
        internal bool TryGetDisplay(byte bank, ushort pointer, out EnemySpritemapParts parts)
        {
            if (EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, pointer) is { } frame)
                return self.TryGetDisplay(frame, out parts);
            parts = EnemySpritemapParts.Empty;
            return false;
        }
    }

    /// <summary>Extended compositions by cartridge address.</summary>
    extension(EnemyExtendedFrameCatalog self)
    {

        /// <summary>The components displayed for the frame stored at <paramref name="pointer"/> in <paramref name="bank"/>.</summary>
        internal bool TryGetDisplay(byte bank, ushort pointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
        {
            if (EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, pointer) is { } frame)
                return self.TryGetDisplay(frame, out components);
            components = default;
            return false;
        }

        /// <summary>The word of the frame displayed for the frame stored at <paramref name="pointer"/> in <paramref name="bank"/>.</summary>
        internal ushort GetDisplayPointer(byte bank, ushort pointer) =>
            EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, pointer) is { } frame
                ? EnemyFrameCartridgeLookupTooling.Word(self.Placements.GetDisplayFrame(frame))
                : pointer;
    }
}
