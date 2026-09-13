using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Composes the native frame and area lettering once, at import, into editable tile grids.</summary>
public static class PauseBackdropExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] frame = RomDataReader.ReadFixedBank(bus, PauseBackdropDefinitions.FrameSource, PauseBackdropDefinitions.ByteCount);
        var areas = new Dictionary<string, PauseBackdropCell[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] image = frame.ToArray();
            ushort label = RomDataReader.ReadWordFixedBank(bus, PauseBackdropDefinitions.LabelPointers + AreaIds.ToIndex(area) * 2);
            RomDataReader.ReadFixedBank(bus, PauseBackdropDefinitions.LabelBank | label, PauseBackdropDefinitions.LabelWords * 2)
                .CopyTo(image, PauseBackdropDefinitions.LabelCell * 2);
            areas.Add(area.ToString(), Cells(image, area.ToString()));
        }
        var buttons = Cells(RomDataReader.ReadFixedBank(bus, PauseBackdropDefinitions.ButtonSource,
            PauseBackdropDefinitions.ButtonCells * 2), "Buttons");
        using var output = new MemoryStream();
        PauseBackdropPresentation.Write(output, new() { Version = PauseBackdropDefinitions.Version, Areas = areas, Buttons = buttons });
        return output.ToArray();

        static PauseBackdropCell[] Cells(byte[] image, string name)
        {
            var cells = new PauseBackdropCell[image.Length / sizeof(ushort)];
            for (int index = 0; index < cells.Length; index++)
            {
                var word = new MapTileWord(BinaryPrimitives.ReadUInt16LittleEndian(image.AsSpan(index * 2)));
                int character = word.CharacterIndex;
                if (character >= PauseBackdropDefinitions.AtlasTileCount * 2)
                    throw new InvalidDataException($"Pause backdrop {name} cell {index} references an unloaded character {character}.");
                cells[index] = new()
                {
                    Atlas = character < PauseBackdropDefinitions.AtlasTileCount ? PauseBackdropDefinitions.MapAtlas : PauseBackdropDefinitions.InterfaceAtlas,
                    TileColumn = character % PauseBackdropDefinitions.AtlasColumns,
                    TileRow = character % PauseBackdropDefinitions.AtlasTileCount / PauseBackdropDefinitions.AtlasColumns,
                    Palette = word.PaletteIndex, Priority = word.HasPriority,
                    FlipX = (word.Raw & MapPresentationFormat.FlipXBit) != 0,
                    FlipY = (word.Raw & MapPresentationFormat.FlipYBit) != 0,
                };
            }
            return cells;
        }
    }
}
