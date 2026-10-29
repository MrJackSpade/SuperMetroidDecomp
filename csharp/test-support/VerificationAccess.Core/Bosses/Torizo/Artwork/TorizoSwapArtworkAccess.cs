using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoSwapArtwork"/> members production does not use.</summary>
internal static class TorizoSwapArtworkAccess
{
    /// <summary>A page's cartridge source, which only verification and extraction read.</summary>
    extension(TorizoTileSheet page)
    {
        /// <summary>The page's first byte on the SNES bus.</summary>
        internal int SourceAddress => TorizoSourceDefinitions.PageSources[page.Page];
    }

    /// <summary>Exposes <see cref="TorizoSwapArtwork"/> state that only verification reads.</summary>
    extension(TorizoSwapArtwork self)
    {
        /// <summary>Finds the selected bytes for a native upload range wholly contained in one tile page.</summary>
        internal bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> characters)
        {
            for (int index = 0; index < self.pages.Length; index++)
            {
                TorizoTileSheet page =
                    TorizoSwapArtworkDefinitions.All[index];
                int offset = sourceAddress - page.SourceAddress;
                if (offset < 0 || byteCount <= 0 ||
                    offset > page.ByteCount - byteCount)
                    continue;
                characters = self.pages[index].Characters.Slice(offset, byteCount);
                return true;
            }
            characters = default;
            return false;
        }
    }
}
