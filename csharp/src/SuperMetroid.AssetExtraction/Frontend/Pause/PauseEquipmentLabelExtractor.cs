using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Extracts pause inventory label artwork and its visual destinations.</summary>
[CpuBusBoundary("codec: ROM bank-$82 pause inventory label tilemaps → pause equipment labels")]
public static class PauseEquipmentLabelExtractor
{
    /// <summary>Resolves native equipment-label destinations and tilemap pointers, verifies the Hyper Beam pointer layout, and serializes editable label cells.</summary>
    /// <param name="bus">Supported-cartridge address space containing equipment label offsets, pointer tables, and tilemap words.</param>
    /// <returns>UTF-8 JSON bytes for normal, blank, disabled, and Hyper Beam equipment labels.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var labels = new Dictionary<string, PauseEquipmentLabel>(StringComparer.Ordinal);
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
        {
            PauseEquipmentCategoryDefinition definition = PauseEquipmentCategories.Get(category);
            PauseEquipmentLabelTables tables = PauseMenuSourceDefinitions.EquipmentLabelTables[category];
            for (int item = 0; item < definition.ItemCount; item++)
            {
                ushort destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), tables.Places + item * 2);
                ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), tables.Labels + item * 2);
                labels.Add(PauseEquipmentLabelDefinitions.Key(category, item), new()
                {
                    Column = ((destination - PauseMenuSourceDefinitions.EquipmentTilemapBase) / 2) % PauseEquipmentLabelDefinitions.TilemapColumns,
                    Row = ((destination - PauseMenuSourceDefinitions.EquipmentTilemapBase) / 2) / PauseEquipmentLabelDefinitions.TilemapColumns,
                    Cells = ReadCells(bus, source, definition.LabelWordCount,
                        PauseEquipmentLabelDefinitions.Key(category, item)),
                });
            }
        }

        ushort hyper = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.HyperBeamLabels +
            PauseEquipmentLabelDefinitions.HyperBeamItem * sizeof(ushort));
        for (int index = 0; index < PauseEquipmentLabelDefinitions.ItemCount(PauseEquipmentCategory.Beams); index++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                PauseMenuSourceDefinitions.HyperBeamLabels + index * sizeof(ushort));
            ushort expected = index == PauseEquipmentLabelDefinitions.HyperBeamItem
                ? hyper : PauseMenuSourceDefinitions.BlankEquipmentLabel;
            if (pointer != expected)
                throw new InvalidDataException("Native Hyper Beam pause pointers do not match the compiled one-label layout.");
        }
        labels.Add(PauseEquipmentLabelDefinitions.HyperKey, new()
        {
            Column = labels[PauseEquipmentLabelDefinitions.Key(PauseEquipmentCategory.Beams, PauseEquipmentLabelDefinitions.HyperBeamItem)].Column,
            Row = labels[PauseEquipmentLabelDefinitions.Key(PauseEquipmentCategory.Beams, PauseEquipmentLabelDefinitions.HyperBeamItem)].Row,
            Cells = ReadCells(bus, hyper, PauseEquipmentLabelDefinitions.EquipmentWords,
                PauseEquipmentLabelDefinitions.HyperKey),
        });

        using var output = new MemoryStream();
        PauseEquipmentLabelPresentation.Write(output, new()
        {
            Version = PauseEquipmentLabelDefinitions.Version,
            DisabledPalette = PauseEquipmentLabelDefinitions.DisabledPalette,
            Blank = ReadCells(bus, PauseMenuSourceDefinitions.BlankEquipmentLabel,
                PauseEquipmentLabelDefinitions.EquipmentWords, "Equipment.Blank"),
            Labels = labels,
        });
        return output.ToArray();
    }

    /// <summary>Decodes a bank-$82 label tilemap into editable atlas coordinates, palette, priority, and flips.</summary>
    private static PauseBackdropCell[] ReadCells(ISnesAddressSpace bus, ushort source, int count, string name) =>
        Enumerable.Range(0, count)
            .Select(index => PauseTileGrid.FromWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                CartridgeBankWindow.At(PauseMenuSourceDefinitions.MenuBank, unchecked((ushort)(source + index * 2)))), $"{name}.{index}"))
            .ToArray();
}
