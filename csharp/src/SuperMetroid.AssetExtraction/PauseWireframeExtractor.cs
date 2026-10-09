using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves wireframe pointers at import, exposing only visual atlas cells to authors.</summary>
public static class PauseWireframeExtractor
{
    /// <summary>Resolves every native suit wireframe pointer and converts its tile words to bounded semantic atlas cells.</summary>
    /// <param name="bus">Supported-cartridge address space containing wireframe pointers and fixed-size tilemaps.</param>
    /// <returns>UTF-8 JSON bytes keyed by wireframe kind.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new Dictionary<string, PauseBackdropCell[]>();
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseWireframeDefinitions.Pointers + (int)kind * 2);
            var cells = new PauseBackdropCell[PauseWireframeDefinitions.Cells];
            for (int index = 0; index < cells.Length; index++)
            {
                var word = new MapTileWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseWireframeDefinitions.Bank | (pointer + index * 2)));
                int character = word.CharacterIndex;
                if (character >= PauseBackdropDefinitions.AtlasTileCount * 2)
                    throw new InvalidDataException($"Wireframe {kind} cell {index} references an unloaded character {character}.");
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
            frames.Add(kind.ToString(), cells);
        }
        using var output = new MemoryStream();
        PauseWireframePresentation.Write(output, new() { Version = PauseWireframeDefinitions.Version, Frames = frames });
        return output.ToArray();
    }
}
