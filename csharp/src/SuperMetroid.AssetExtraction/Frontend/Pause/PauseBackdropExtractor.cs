using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Buffers.Binary;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Composes the native frame and area lettering once, at import, into editable tile grids.</summary>
public static class PauseBackdropExtractor
{
    /// <summary>Combines the native pause frame with each area label, converts every tile word to semantic atlas coordinates, and serializes the shared button strip.</summary>
    /// <param name="bus">Supported-cartridge address space containing the pause frame, area-label pointers, and button tilemap.</param>
    /// <returns>UTF-8 JSON bytes for all area backdrops and pause buttons.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] frame = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseFrameTilemap, PauseBackdropDefinitions.ByteCount);
        var areas = new Dictionary<string, PauseBackdropCell[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] image = frame.ToArray();
            int label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseAreaLabelPointers.AddWithinBank(AreaIds.ToIndex(area) * 2));
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), SnesAddress.InBank(PauseMenuSourceDefinitions.PauseAreaLabelBank.Bank, label), PauseBackdropDefinitions.LabelWords * 2)
                .CopyTo(image, PauseBackdropDefinitions.LabelCell * 2);
            areas.Add(area.ToString(), Cells(image, area.ToString()));
        }
        var buttons = Cells(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.PauseButtonTilemap,
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
                    Atlas = character < PauseBackdropDefinitions.AtlasTileCount ? PauseAtlas.Map : PauseAtlas.Interface,
                    TileColumn = character % PauseBackdropDefinitions.AtlasColumns,
                    TileRow = character % PauseBackdropDefinitions.AtlasTileCount / PauseBackdropDefinitions.AtlasColumns,
                    Palette = word.PaletteIndex, Priority = word.HasPriority,
                    FlipX = (word.Mirroring & TileFlip.Horizontal) != TileFlip.None,
                    FlipY = (word.Mirroring & TileFlip.Vertical) != TileFlip.None,
                };
            }
            return cells;
        }
    }
}
