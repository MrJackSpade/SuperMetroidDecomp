using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Exports fixed Ceres warning tile words without exposing DMA control metadata.</summary>
internal static class CeresEscapeOverlayTilemapFiles
{
    /// <summary>Exports every fixed warning page's tile words.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var pages = new Dictionary<string, BackgroundTile[]>();
        foreach (CeresEscapeOverlayTilemapDefinition page in
                 CeresEscapeOverlayPages.All)
        {
            var source = new ushort[page.WordCount];
            SnesAddress first = Source(page);
            for (int index = 0; index < source.Length; index++)
                source[index] = CartridgeImportSource.Require(bus).ReadCartridgeWord((int)first.AddWithinBank(index * sizeof(ushort)));
            pages.Add(page.Name, BackgroundTileWords.ToTiles(source));
        }
        byte[] json = CeresEscapeOverlays.Write(
            new CeresEscapeOverlayTilemapDocument
            {
                Version = CeresWarningGlyphDefinitions.Version,
                Pages = pages,
            });
        return json;
    }

    /// <summary>
    /// Where the cartridge stores <paramref name="page"/>'s words: the title on its own, the Japanese subtitle's row
    /// halves one after another in their writing order.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="page"/> is not one of the overlays.</exception>
    internal static SnesAddress Source(CeresEscapeOverlayTilemapDefinition page)
    {
        if (page == CeresEscapeOverlayTilemapDefinitions.Emergency)
            return CeresSourceDefinitions.EscapeOverlay.Emergency;
        int offset = 0;
        foreach (CeresEscapeOverlayTilemapDefinition row in CeresEscapeOverlayTilemapDefinitions.Japanese)
        {
            if (row == page)
                return CeresSourceDefinitions.EscapeOverlay.JapaneseRows.AddWithinBank(offset);
            offset += row.WordCount * sizeof(ushort);
        }
        throw new InvalidDataException($"{page.Name} is not a Ceres escape overlay.");
    }
}
