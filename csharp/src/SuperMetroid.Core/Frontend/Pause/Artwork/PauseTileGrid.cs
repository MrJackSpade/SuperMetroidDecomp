using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Compiles shared pause-art atlas cells into the tiles they place, without interpreting controls or inventory.</summary>
internal static class PauseTileGrid
{
    /// <summary>Validates atlas cells and places the tile each selects.</summary>
    public static BackgroundTile[] Compile(PauseBackdropCell[] cells, string name)
    {
        var tiles = new BackgroundTile[cells.Length];
        for (int index = 0; index < cells.Length; index++)
        {
            var cell = cells[index];
            if (cell is null || (uint)cell.TileColumn >= PauseBackdropDefinitions.AtlasColumns ||
                (uint)cell.TileRow >= PauseBackdropDefinitions.AtlasRows || (uint)cell.Palette >= MapPresentationFormat.PaletteCount ||
                !Enum.IsDefined(cell.Atlas))
                throw new InvalidDataException($"Pause artwork {name} cell {index} has an invalid atlas, coordinate or palette.");
            int character = cell.TileRow * PauseBackdropDefinitions.AtlasColumns + cell.TileColumn;
            if (cell.Atlas == PauseAtlas.Interface) character += PauseBackdropDefinitions.AtlasTileCount;
            tiles[index] = new BackgroundTile(character, cell.Palette, cell.FlipX, cell.FlipY, cell.Priority);
        }
        return tiles;
    }
}
