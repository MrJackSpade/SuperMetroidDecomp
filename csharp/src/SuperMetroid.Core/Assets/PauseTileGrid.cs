using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>Compiles shared pause-art atlas references without interpreting controls or inventory.</summary>
internal static class PauseTileGrid
{
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
