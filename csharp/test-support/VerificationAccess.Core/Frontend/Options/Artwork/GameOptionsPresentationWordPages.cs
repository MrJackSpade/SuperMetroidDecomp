using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// The options pages as the cartridge's little-endian tilemap words, so verification can compare the port's pages with
/// the cartridge's bytes.
/// </summary>
internal static class GameOptionsPresentationWordPages
{
    /// <summary>Options pages as tilemap word bytes.</summary>
    extension(GameOptionsPresentation self)
    {
        /// <summary>An independent copy of one compiled page, as tilemap word bytes.</summary>
        internal byte[] CreatePageBytes(GameOptionsTilemap tilemap) => BackgroundTileWordBytes.Of(self.CreatePage(tilemap));

        /// <summary>Recolours the language regions of a page held as tilemap word bytes.</summary>
        internal void ApplyLanguage(byte[] primaryPage, bool japanese) => Change(primaryPage, tiles => self.ApplyLanguage(tiles, japanese));

        /// <summary>Writes one controller label into a page held as tilemap word bytes.</summary>
        internal void ApplyControllerLabel(byte[] page, int action, int button) => Change(page, tiles => self.ApplyControllerLabel(tiles, action, button));

        /// <summary>Recolours one special setting's labels in a page held as tilemap word bytes.</summary>
        internal void ApplySpecialToggle(byte[] page, GameOptionsToggle name, bool enabled) =>
            Change(page, tiles => self.ApplySpecialToggle(tiles, name, enabled));
    }

    /// <summary>Applies <paramref name="change"/> to the tiles <paramref name="page"/>'s words place and writes them back.</summary>
    private static void Change(byte[] page, Action<BackgroundTile[]> change)
    {
        BackgroundTile[] tiles = BackgroundTileWordBytes.Tiles(page);
        change(tiles);
        BackgroundTileWordBytes.Of(tiles).CopyTo(page, 0);
    }
}
