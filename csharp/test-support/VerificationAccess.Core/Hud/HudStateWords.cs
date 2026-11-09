using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hud.Artwork;

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

        /// <summary>
        /// Each difference between the HUD's drawn rows and the cartridge's <paramref name="native"/> rows: a minimap cell by
        /// the map cell and shade it shows (#1513), since one picture of a map cell stands for every map word that draws it,
        /// and every other cell by its word.
        /// </summary>
        /// <param name="native">The cartridge's three drawn rows, row-major.</param>
        /// <exception cref="InvalidDataException">A native minimap word is not one the minimap writes.</exception>
        internal List<string> NativeRowDifferences(ushort[] native)
        {
            Ensure.LengthEqual<ushort>(native, HudStateConstants.MutableTileCount);
            ushort[] port = hud.TileWords;
            var minimapCells = new Dictionary<int, int>();
            // Before the minimap's first update its cells show the template, compared word for word like every other cell.
            if (hud.Presentation is { } presentation && !hud.Minimap.IsEmpty)
            {
                int columns = GameplayHudPresentation.MinimapSize.X;
                for (int index = 0; index < hud.Minimap.Length; index++)
                    minimapCells.Add(GameplayHudArtwork.Of(presentation).MinimapCellIndex(
                        new Position16<TilemapCells>(checked((ushort)(index % columns)), checked((ushort)(index / columns)))), index);
            }
            var differences = new List<string>();
            for (int tile = 0; tile < native.Length; tile++)
            {
                if (minimapCells.TryGetValue(tile, out int cell))
                {
                    MinimapCell expected = HudNativeState.Decode(new MapTileWord(native[tile]));
                    if (expected != hud.Minimap[cell])
                        differences.Add($"HUD minimap cell {cell}: native={native[tile]:X4} ({expected}) port={hud.Minimap[cell]}");
                }
                else if (port[tile] != native[tile])
                    differences.Add($"HUD tile {tile}: native={native[tile]:X4} port={port[tile]:X4}");
            }
            return differences;
        }
    }
}
