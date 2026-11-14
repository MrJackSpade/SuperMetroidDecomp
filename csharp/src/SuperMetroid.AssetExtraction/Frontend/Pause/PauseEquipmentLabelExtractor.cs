using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Extracts pause inventory label artwork and its visual destinations.</summary>
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
                int destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), tables.Places + item * 2);
                int source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), tables.Labels + item * 2);
                labels.Add(PauseEquipmentLabelKeys.Key(category, item), new()
                {
                    Column = ((destination - PauseMenuSourceDefinitions.EquipmentTilemapBase) / 2) % PauseEquipmentLabelArtworkDefinitions.TilemapColumns,
                    Row = ((destination - PauseMenuSourceDefinitions.EquipmentTilemapBase) / 2) / PauseEquipmentLabelArtworkDefinitions.TilemapColumns,
                    Cells = ReadCells(bus, source, PauseEquipmentLabelKeys.WordCount(category),
                        PauseEquipmentLabelKeys.Key(category, item)),
                });
            }
        }

        int hyper = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.HyperBeamLabels +
            PauseEquipmentLabelArtworkDefinitions.HyperBeamItem * sizeof(ushort));
        for (int index = 0; index < PauseEquipmentLabels.ItemCount(PauseEquipmentCategory.Beams); index++)
        {
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                PauseMenuSourceDefinitions.HyperBeamLabels + index * sizeof(ushort));
            int expected = index == PauseEquipmentLabelArtworkDefinitions.HyperBeamItem
                ? hyper : PauseMenuSourceDefinitions.BlankEquipmentLabel;
            if (pointer != expected)
                throw new InvalidDataException("Native Hyper Beam pause pointers do not match the compiled one-label layout.");
        }
        labels.Add(PauseEquipmentLabelArtworkDefinitions.HyperKey, new()
        {
            Column = labels[PauseEquipmentLabelKeys.Key(PauseEquipmentCategory.Beams, PauseEquipmentLabelArtworkDefinitions.HyperBeamItem)].Column,
            Row = labels[PauseEquipmentLabelKeys.Key(PauseEquipmentCategory.Beams, PauseEquipmentLabelArtworkDefinitions.HyperBeamItem)].Row,
            Cells = ReadCells(bus, hyper, PauseEquipmentLabelArtworkDefinitions.EquipmentWords,
                PauseEquipmentLabelArtworkDefinitions.HyperKey),
        });

        using var output = new MemoryStream();
        PauseEquipmentLabelPresentation.Write(output, new()
        {
            Version = PauseEquipmentLabelArtworkDefinitions.Version,
            DisabledPalette = PauseEquipmentLabelArtworkDefinitions.DisabledPalette,
            Blank = ReadCells(bus, PauseMenuSourceDefinitions.BlankEquipmentLabel,
                PauseEquipmentLabelArtworkDefinitions.EquipmentWords, "Equipment.Blank"),
            Labels = labels,
        });
        return output.ToArray();
    }

    /// <summary>Decodes a bank-$82 label tilemap into editable atlas coordinates, palette, priority, and flips.</summary>
    private static PauseBackdropCell[] ReadCells(ISnesAddressSpace bus, int source, int count, string name) =>
        Enumerable.Range(0, count)
            .Select(index => PauseTileWords.FromWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                CartridgeBankWindow.At(PauseMenuSourceDefinitions.MenuBank, unchecked((ushort)(source + index * 2)))), $"{name}.{index}"))
            .ToArray();
}
