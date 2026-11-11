using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// The options pages as the cartridge's little-endian tilemap words, so verification can compare the port's pages with
/// the cartridge's bytes.
/// </summary>
internal static class GameOptionsPresentationWordPages
{
    /// <summary>Options pages as tilemap word bytes.</summary>
    extension(GameOptionsSourcePresentation presentation)
    {
        /// <summary>An independent copy of one compiled page, as tilemap word bytes.</summary>
        internal byte[] CreatePageBytes(GameOptionsScreen tilemap) => BackgroundTileWordBytes.Of(presentation.CreatePage(tilemap));

        /// <summary>Recolours the language regions of a page held as tilemap word bytes.</summary>
        internal void ApplyLanguage(byte[] primaryPage, bool japanese) => Change(primaryPage, tiles => presentation.ApplyLanguage(tiles, japanese));

        /// <summary>Writes one controller label into a page held as tilemap word bytes.</summary>
        internal void ApplyControllerLabel(byte[] page, int action, int button) => Change(page, tiles => presentation.ApplyControllerLabel(tiles, action, button));

        /// <summary>Recolours one special setting's labels in a page held as tilemap word bytes.</summary>
        internal void ApplySpecialToggle(byte[] page, GameOptionsToggle name, bool enabled) =>
            Change(page, tiles => presentation.ApplySpecialToggle(tiles, name, enabled));
    }

    /// <summary>Applies <paramref name="change"/> to the tiles <paramref name="page"/>'s words place and writes them back.</summary>
    private static void Change(byte[] page, Action<BackgroundTile[]> change)
    {
        BackgroundTile[] tiles = BackgroundTileWordBytes.Tiles(page);
        change(tiles);
        BackgroundTileWordBytes.Of(tiles).CopyTo(page, 0);
    }
}
