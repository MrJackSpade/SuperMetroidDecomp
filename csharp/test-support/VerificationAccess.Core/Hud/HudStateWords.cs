using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Reads the HUD's three drawn rows as tiles and as the cartridge's packed tilemap words, so verification can compare
/// what the HUD's values draw with the words the cartridge writes to <c>$7E:C608</c>.
/// </summary>
internal static class HudStateWords
{
    /// <summary>The HUD's drawn rows.</summary>
    extension(HudState hud)
    {
        /// <summary>The three rows below the static top row as the installed artwork draws the HUD's values, row-major.</summary>
        internal BackgroundTile[] Tiles => hud.Rows()[HudState.WidthInTiles..];

        /// <summary>A copy of each drawn cell's packed tilemap word, row-major.</summary>
        internal ushort[] TileWords => BackgroundTileWordBytes.Words(hud.Tiles);
    }
}
