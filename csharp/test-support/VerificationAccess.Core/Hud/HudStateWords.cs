using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Reads the HUD's three drawn rows as tiles and as the cartridge's packed tilemap words, so verification can compare
/// what the HUD's values place, laid out from the installed images' tiles (#1466), with the words the cartridge writes to
/// <c>$7E:C608</c>.
/// </summary>
internal static class HudStateWords
{
    /// <summary>The HUD's drawn rows.</summary>
    extension(HudState hud)
    {
        /// <summary>
        /// The three rows below the static top row as the HUD's placed images' tiles lay them out, row-major; empty tiles
        /// while no artwork is bound.
        /// </summary>
        internal BackgroundTile[] Tiles => hud.Presentation is { } presentation
            ? GameplayHudArtwork.Of(presentation).Rows(hud.Shown())[HudStateConstants.WidthInTiles..]
            : new BackgroundTile[HudStateConstants.MutableTileCount];

        /// <summary>A copy of each drawn cell's packed tilemap word, row-major.</summary>
        internal ushort[] TileWords => BackgroundTileWordBytes.Words(hud.Tiles);
    }
}
