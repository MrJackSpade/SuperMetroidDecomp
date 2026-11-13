using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Reads the HUD's three drawn rows as tiles and as the cartridge's packed tilemap words, so verification can compare
/// what the HUD's values place, laid out from the installed images' tiles (#1466), with the words the cartridge writes to
/// <c>$7E:C608</c>. The tiles are the import's (#1616): each read names the installed HUD artwork the HUD's layout was read
/// with.
/// </summary>
internal static class HudStateWords
{
    /// <summary>The HUD's drawn rows.</summary>
    extension(HudState hud)
    {
        /// <summary>
        /// The three rows below the static top row as the HUD's placed images' tiles in <paramref name="artwork"/> lay them
        /// out, row-major; empty tiles while no layout is bound.
        /// </summary>
        /// <param name="artwork">The installed HUD artwork the bound layout was read with.</param>
        internal BackgroundTile[] Tiles(GameplayHudArtwork artwork) => hud.Presentation is not null
            ? Ensure.NotNull(artwork).Rows(hud.Shown())[HudStateConstants.WidthInTiles..]
            : new BackgroundTile[HudStateConstants.MutableTileCount];

        /// <summary>A copy of each drawn cell's packed tilemap word, row-major, laid out from <paramref name="artwork"/>'s tiles.</summary>
        /// <param name="artwork">The installed HUD artwork the bound layout was read with.</param>
        internal ushort[] TileWords(GameplayHudArtwork artwork) => BackgroundTileWordBytes.Words(hud.Tiles(artwork));

        /// <summary>
        /// Each difference between the HUD's drawn rows and the cartridge's <paramref name="native"/> rows: a minimap cell by
        /// the map cell and shade it shows (#1513), since one picture of a map cell stands for every map word that draws it,
        /// and every other cell by its word.
        /// </summary>
        /// <param name="native">The cartridge's three drawn rows, row-major.</param>
        /// <param name="artwork">The installed HUD artwork the bound layout was read with.</param>
        /// <exception cref="InvalidDataException">A native minimap word is not one the minimap writes.</exception>
        internal List<string> NativeRowDifferences(ushort[] native, GameplayHudArtwork artwork)
        {
            Ensure.LengthEqual<ushort>(native, HudStateConstants.MutableTileCount);
            ushort[] port = hud.TileWords(artwork);
            var minimapCells = new Dictionary<int, int>();
            // Before the minimap's first update its cells show the template, compared word for word like every other cell.
            if (hud.Presentation is { } presentation && !hud.Minimap.IsEmpty)
            {
                int columns = GameplayHudPresentation.MinimapSize.X;
                for (int index = 0; index < hud.Minimap.Length; index++)
                    minimapCells.Add(presentation.MinimapCellIndex(
                        new Position16<MapTiles>(checked((ushort)(index % columns)), checked((ushort)(index / columns)))), index);
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
