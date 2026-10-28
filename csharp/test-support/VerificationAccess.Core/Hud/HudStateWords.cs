using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Reads the HUD's three mutable rows as the cartridge's packed tilemap words, so verification can compare them with the
/// words the cartridge writes to <c>$7E:C608</c>.
/// </summary>
internal static class HudStateWords
{
    /// <summary>The packed words of the HUD's tiles.</summary>
    extension(HudState hud)
    {
        /// <summary>A copy of each mutable cell's packed tilemap word, row-major.</summary>
        internal ushort[] TileWords => BackgroundTileWordBytes.Words(hud.Tiles);
    }
}
