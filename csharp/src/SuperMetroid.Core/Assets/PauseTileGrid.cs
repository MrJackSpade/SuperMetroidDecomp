using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>Compiles shared pause-art atlas references without interpreting controls or inventory.</summary>
internal static class PauseTileGrid
{
    /// <summary>Decodes one SNES map word into the pause-art atlas, tile coordinates, palette, priority, and flip settings.</summary>
    /// <param name="raw">Encoded map word whose character index selects a tile in the combined pause atlases.</param>
    /// <param name="name">Asset label included in the error when the word references a character outside the loaded atlases.</param>
    /// <returns>The visual cell represented by <paramref name="raw"/>.</returns>
    /// <exception cref="InvalidDataException">The character index is not present in the loaded pause-art atlases.</exception>
    public static PauseBackdropCell FromWord(ushort raw, string name)
    {
        var word = new Game.MapTileWord(raw);
        int character = word.CharacterIndex;
        if (character >= PauseBackdropDefinitions.AtlasTileCount * 2)
            throw new InvalidDataException($"Pause artwork {name} references unloaded character {character}.");
        return new()
        {
            Atlas = character < PauseBackdropDefinitions.AtlasTileCount
                ? PauseBackdropDefinitions.MapAtlas : PauseBackdropDefinitions.InterfaceAtlas,
            TileColumn = character % PauseBackdropDefinitions.AtlasColumns,
            TileRow = character % PauseBackdropDefinitions.AtlasTileCount / PauseBackdropDefinitions.AtlasColumns,
            Palette = word.PaletteIndex,
            Priority = word.HasPriority,
            FlipX = (word.Raw & MapPresentationFormat.FlipXBit) != 0,
            FlipY = (word.Raw & MapPresentationFormat.FlipYBit) != 0,
        };
    }

    /// <summary>Encodes pause-art cells as little-endian SNES map words in their input order.</summary>
    /// <param name="cells">Cells to encode; each must select a loaded atlas, an in-range tile coordinate, and a valid palette.</param>
    /// <param name="name">Asset label included in the error when a cell is invalid.</param>
    /// <returns>Two bytes per cell, with the tile character, palette, priority, and flips packed into each word.</returns>
    /// <exception cref="InvalidDataException">A cell is null or contains an unsupported atlas, tile coordinate, or palette.</exception>
    public static byte[] Compile(PauseBackdropCell[] cells, string name)
    {
        var bytes = new byte[cells.Length * sizeof(ushort)];
        for (int index = 0; index < cells.Length; index++)
        {
            var cell = cells[index];
            if (cell is null || (uint)cell.TileColumn >= PauseBackdropDefinitions.AtlasColumns ||
                (uint)cell.TileRow >= PauseBackdropDefinitions.AtlasRows || (uint)cell.Palette >= MapPresentationFormat.PaletteCount ||
                cell.Atlas is not (PauseBackdropDefinitions.MapAtlas or PauseBackdropDefinitions.InterfaceAtlas))
                throw new InvalidDataException($"Pause artwork {name} cell {index} has an invalid atlas, coordinate or palette.");
            int character = cell.TileRow * PauseBackdropDefinitions.AtlasColumns + cell.TileColumn;
            if (cell.Atlas == PauseBackdropDefinitions.InterfaceAtlas) character += PauseBackdropDefinitions.AtlasTileCount;
            ushort word = (ushort)(character | cell.Palette << MapPresentationFormat.PaletteShift |
                (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) | (cell.FlipY ? MapPresentationFormat.FlipYBit : 0));
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), word);
        }
        return bytes;
    }
}
