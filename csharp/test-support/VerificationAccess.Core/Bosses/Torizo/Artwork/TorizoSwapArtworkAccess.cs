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
            foreach (TorizoTileSheet page in TorizoSwapArtworkDefinitions.All)
            {
                int offset = sourceAddress - page.SourceAddress;
                if (offset < 0 || byteCount <= 0 ||
                    offset > page.ByteCount - byteCount)
                    continue;
                characters = self.pages[page.Page].Characters.Slice(offset, byteCount);
                return true;
            }
            characters = default;
            return false;
        }
    }

    /// <summary>Exposes <see cref="TorizoSwapArtwork"/> state that only verification reads.</summary>
    extension(TorizoSwapArtwork self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("TorizoInstructionVramArtwork-v1", content =>
            {
                content.Append("pages", self.pages.Count);
                foreach (TorizoTileSheet page in TorizoSwapArtworkDefinitions.All)
                    content.Append("tiles", self.pages[page.Page].Characters.Span);
            });
    }
}
