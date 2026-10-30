using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// Verification's native view of the Ceres escape's installed pages (#1415): where the cartridge stores each character
/// page and overlay. Core names them by page; a ROM oracle or native transfer translates through these.
/// </summary>
internal static class CeresEscapeNativeSources
{
    /// <summary>Where character page <paramref name="page"/> (<see cref="CeresEscapeTileArtworkDefinitions.All"/>) starts on the bus.</summary>
    internal static int PageSource(int page) => (int)EscapeSourceDefinitions.CeresEscapePages[page];

    /// <summary>Where overlay <paramref name="page"/>'s words start on the bus.</summary>
    internal static int OverlaySource(CeresEscapeOverlayTilemapDefinition page) => (int)CeresEscapeOverlayTilemapFiles.Source(page);

    /// <summary>Resolves a native transfer from the bus against the installed Ceres escape character pages.</summary>
    extension(CeresEscapeTileArtwork self)
    {
        /// <summary>The installed characters a native transfer of <paramref name="byteCount"/> bytes from <paramref name="sourceAddress"/> copies, when it lies wholly inside one page.</summary>
        internal bool TryResolveNative(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> characters)
        {
            for (int page = 0; page < CeresEscapeTileArtworkDefinitions.All.Length; page++)
                if (self.TryResolve(page, sourceAddress - PageSource(page), byteCount, out characters))
                    return true;
            characters = default;
            return false;
        }
    }

    /// <summary>Resolves a native transfer from the bus against the installed Ceres escape overlays.</summary>
    extension(CeresEscapeOverlays self)
    {
        /// <summary>The installed words a native transfer of <paramref name="byteCount"/> bytes from <paramref name="sourceAddress"/> copies, when it is exactly one overlay page.</summary>
        internal bool TryResolveNative(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
                if (OverlaySource(page) == sourceAddress && page.WordCount * sizeof(ushort) == byteCount)
                {
                    bool resolved = self.TryResolve(page, out ReadOnlyMemory<SuperMetroid.AssetExtraction.Artwork.BackgroundTile> tiles);
                    data = SuperMetroid.Core.Assets.BackgroundTileWordBytes.Of(tiles.Span);
                    return resolved;
                }
            data = default;
            return false;
        }
    }
}
