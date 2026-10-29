using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// Lays the HUD's images out as tiles (#1466): each writer places the images the HUD's layout places for a value and
/// writes each image's tiles where it is placed, so verification compares what the game places with the words the
/// cartridge's HUD routines write.
/// </summary>
internal static class GameplayHudArtworkAccess
{
    /// <summary>The HUD's images as tiles.</summary>
    extension(GameplayHudArtwork artwork)
    {
        /// <summary>The HUD's static top row, with any installed edits (#1348).</summary>
        internal BackgroundTile[] TopRowTiles() => [.. artwork.Tiles(HudTopRowImage.Instance).Tiles];

        /// <summary>
        /// The HUD's four rows as <paramref name="hud"/> shows them, row by row: each placed image's tiles over the earlier
        /// ones, and the empty tile where no image is placed.
        /// </summary>
        internal BackgroundTile[] Rows(ShownHud hud) => artwork.RowsOver(hud, default);

        /// <summary>
        /// The HUD's four rows as <paramref name="hud"/> shows them, row by row, over <paramref name="uncovered"/> where no
        /// image is placed.
        /// </summary>
        internal BackgroundTile[] RowsOver(ShownHud hud, BackgroundTile uncovered)
        {
            var rows = new BackgroundTile[GameplayHudDefinitions.Width * GameplayHudDefinitions.Rows];
            rows.AsSpan().Fill(uncovered);
            foreach (PlacedHudImage placed in Ensure.NotNull(hud).Images)
                artwork.Write(rows, placed, 0);
            return rows;
        }

        /// <summary>Draws one energy tank full or empty into the three drawn rows.</summary>
        internal void DrawEnergyTank(Span<BackgroundTile> tiles, int tank, HudEnergyTank shown) =>
            artwork.Write(tiles, artwork.EnergyTank(tank, shown), 1);

        /// <summary>Draws the two-digit energy count into the three drawn rows.</summary>
        internal void DrawEnergyCount(Span<BackgroundTile> tiles, int remainder)
        {
            foreach (PlacedHudImage placed in artwork.EnergyCount(remainder))
                artwork.Write(tiles, placed, 1);
        }

        /// <summary>Draws an ammunition count into the three drawn rows.</summary>
        internal void ApplyAmmo(Span<BackgroundTile> tiles, int itemIndex, int value)
        {
            foreach (PlacedHudImage placed in artwork.Ammo(itemIndex, value))
                artwork.Write(tiles, placed, 1);
        }

        /// <summary>Draws the AUTO reserve indicator as <paramref name="indicator"/> shows it into the three drawn rows.</summary>
        internal void ApplyAutoReserve(Span<BackgroundTile> tiles, HudReserveIndicator indicator)
        {
            foreach (PlacedHudImage placed in artwork.AutoReserve(indicator))
                artwork.Write(tiles, placed, 1);
        }

        /// <summary>Clears the AUTO reserve indicator in the three drawn rows.</summary>
        internal void ClearAutoReserve(Span<BackgroundTile> tiles) => artwork.ApplyAutoReserve(tiles, HudReserveIndicator.Cleared);

        /// <summary>The cell index in the three drawn rows of minimap cell <paramref name="output"/>.</summary>
        internal int MinimapCellIndex(Position16<TilemapCells> output)
        {
            Position16<TilemapCells> cell = artwork.MinimapCell(output, MinimapHiddenCell.Instance).Cell;
            return (cell.Y - 1) * GameplayHudDefinitions.Width + cell.X;
        }

        /// <summary>Draws one minimap cell into the three drawn rows.</summary>
        internal void DrawMinimapCell(Span<BackgroundTile> tiles, Position16<TilemapCells> output, MinimapCell cell) =>
            artwork.Write(tiles, artwork.MinimapCell(output, cell), 1);

        /// <summary>
        /// Writes <paramref name="placed"/>'s tiles into <paramref name="tiles"/>, rows of the HUD from
        /// <paramref name="firstRow"/>.
        /// </summary>
        private void Write(Span<BackgroundTile> tiles, PlacedHudImage placed, int firstRow)
        {
            HudImageTiles image = artwork.Tiles(placed.Image);
            int rows = tiles.Length / GameplayHudDefinitions.Width;
            for (int y = 0; y < image.Size.Y; y++)
            for (int x = 0; x < image.Size.X; x++)
            {
                int row = placed.Cell.Y + y - firstRow, column = placed.Cell.X + x;
                if (row < 0 || row >= rows || column >= GameplayHudDefinitions.Width)
                    throw new InvalidDataException($"HUD image {placed.Image.FrameName} runs past the rows written.");
                tiles[row * GameplayHudDefinitions.Width + column] = image.Tiles[y * image.Size.X + x];
            }
        }
    }
}
