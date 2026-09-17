using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts pause inventory label artwork and its visual destinations.</summary>
public static class PauseEquipmentLabelExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var labels = new Dictionary<string, PauseEquipmentLabel>(StringComparer.Ordinal);
        for (int category = 1; category <= 3; category++)
        {
            PauseEquipmentCategoryDefinition definition = PauseEquipmentCategories.Definitions[category];
            for (int item = 0; item < definition.ItemCount; item++)
            {
                ushort destination = RomDataReader.ReadWordFixedBank(bus, definition.OffsetTableAddress + item * 2);
                ushort source = RomDataReader.ReadWordFixedBank(bus, definition.TilemapPointerTableAddress + item * 2);
                labels.Add(PauseEquipmentLabelDefinitions.Key(category, item), new()
                {
                    Column = ((destination - PauseEquipmentCategories.TilemapWramBase) / 2) % PauseEquipmentLabelDefinitions.TilemapColumns,
                    Row = ((destination - PauseEquipmentCategories.TilemapWramBase) / 2) / PauseEquipmentLabelDefinitions.TilemapColumns,
                    Cells = ReadCells(bus, source, definition.LabelWordCount,
                        PauseEquipmentLabelDefinitions.Key(category, item)),
                });
            }
        }

        ushort hyper = RomDataReader.ReadWordFixedBank(bus, PauseEquipmentLabelDefinitions.HyperPointerTable +
            PauseEquipmentLabelDefinitions.HyperBeamItem * sizeof(ushort));
        for (int index = 0; index < PauseEquipmentLabelDefinitions.Keys[1].Length; index++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                PauseEquipmentLabelDefinitions.HyperPointerTable + index * sizeof(ushort));
            ushort expected = index == PauseEquipmentLabelDefinitions.HyperBeamItem
                ? hyper : PauseEquipmentLabelDefinitions.BlankSource;
            if (pointer != expected)
                throw new InvalidDataException("Native Hyper Beam pause pointers do not match the compiled one-label layout.");
        }
        labels.Add(PauseEquipmentLabelDefinitions.HyperKey, new()
        {
            Column = labels[PauseEquipmentLabelDefinitions.Key(1, PauseEquipmentLabelDefinitions.HyperBeamItem)].Column,
            Row = labels[PauseEquipmentLabelDefinitions.Key(1, PauseEquipmentLabelDefinitions.HyperBeamItem)].Row,
            Cells = ReadCells(bus, hyper, PauseEquipmentLabelDefinitions.EquipmentWords,
                PauseEquipmentLabelDefinitions.HyperKey),
        });

        using var output = new MemoryStream();
        PauseEquipmentLabelPresentation.Write(output, new()
        {
            Version = PauseEquipmentLabelDefinitions.Version,
            DisabledPalette = PauseEquipmentLabelDefinitions.DisabledPalette,
            Blank = ReadCells(bus, PauseEquipmentLabelDefinitions.BlankSource,
                PauseEquipmentLabelDefinitions.EquipmentWords, "Equipment.Blank"),
            Labels = labels,
        });
        return output.ToArray();
    }

    private static PauseBackdropCell[] ReadCells(ISnesAddressSpace bus, ushort source, int count, string name) =>
        Enumerable.Range(0, count)
            .Select(index => PauseTileGrid.FromWord(RomDataReader.ReadWordFixedBank(bus,
                0x820000 | unchecked((ushort)(source + index * 2))), $"{name}.{index}"))
            .ToArray();
}
