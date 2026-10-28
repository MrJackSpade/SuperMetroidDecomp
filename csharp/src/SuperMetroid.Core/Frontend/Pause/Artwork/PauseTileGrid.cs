using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Compiles shared pause-art atlas references without interpreting controls or inventory.</summary>
internal static class PauseTileGrid
{
    /// <summary>Decodes a packed pause tile into its selected map or interface atlas cell and display attributes.</summary>
    /// <param name="raw">The packed pause tile, 0 through 65535.</param>
    /// <param name="name">The artwork's name, for errors.</param>
    public static PauseBackdropCell FromWord(int raw, string name)
    {
        var word = new SuperMetroid.Core.Map.MapTileWord(checked((ushort)raw));
        int character = word.CharacterIndex;
        if (character >= PauseBackdropDefinitions.AtlasTileCount * 2)
            throw new InvalidDataException($"Pause artwork {name} references unloaded character {character}.");
        return new()
        {
            Atlas = character < PauseBackdropDefinitions.AtlasTileCount ? PauseAtlas.Map : PauseAtlas.Interface,
            TileColumn = character % PauseBackdropDefinitions.AtlasColumns,
            TileRow = character % PauseBackdropDefinitions.AtlasTileCount / PauseBackdropDefinitions.AtlasColumns,
            Palette = word.PaletteIndex,
            Priority = word.HasPriority,
            FlipX = (word.Raw & MapPresentationFormat.FlipXBit) != 0,
            FlipY = (word.Raw & MapPresentationFormat.FlipYBit) != 0,
        };
    }

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
