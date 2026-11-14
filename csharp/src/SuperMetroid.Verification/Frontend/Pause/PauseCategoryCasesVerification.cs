using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause inventory category tables, bounds, and copied label widths against native instructions.</summary>
internal static class PauseCategoryCasesVerification
{
    /// <summary>Runs offset, tilemap, item-count, and label-length checks with reserve and invalid-category cases.</summary>
    internal static void VerifyPauseCategoryCases(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyPauseCategoryOffsets), () => VerifyPauseCategoryOffsets(rom));
        Suite(nameof(VerifyPauseCategoryTilemapPointers), () => VerifyPauseCategoryTilemapPointers(rom));
        Suite(nameof(VerifyPauseCategoryItemCounts), () => VerifyPauseCategoryItemCounts(rom));
        Suite(nameof(VerifyPauseCategoryCopyLengths), () => VerifyPauseCategoryCopyLengths(rom));
        foreach (var column in PauseMenuSourceDefinitions.SelectorColumns)
            AssertTrue(column.All(control => control.Category == column[0].Category), "each column's controls stand in that column's category");
    }

    /// <summary>
    /// Asserts that the label-place table the import reads for each inventory category is the one the cartridge's
    /// category table at $82:C02C names (it asserted the Core catalog's address before #1416).
    /// </summary>
    private static void VerifyPauseCategoryOffsets(ISnesAddressSpace rom)
    {
        foreach (var (category, table) in InventoryTables())
            AssertEqual(0x820000 | ReadVerificationWord(rom, 0x82c02c + 2 * table),
                PauseMenuSourceDefinitions.EquipmentLabelTables[category].Places, "import reads the cartridge's category label places");
    }

    /// <summary>
    /// Asserts that the label-tilemap table the import reads for each inventory category is the one the cartridge's
    /// category table at $82:C044 names (it asserted the Core catalog's address before #1416).
    /// </summary>
    private static void VerifyPauseCategoryTilemapPointers(ISnesAddressSpace rom)
    {
        foreach (var (category, table) in InventoryTables())
            AssertEqual(0x820000 | ReadVerificationWord(rom, 0x82c044 + 2 * table),
                PauseMenuSourceDefinitions.EquipmentLabelTables[category].Labels, "import reads the cartridge's category label tilemaps");
    }

    /// <summary>Matches each category's item bound to its native inventory-scan comparison.</summary>
    private static void VerifyPauseCategoryItemCounts(ISnesAddressSpace rom)
    {
        // Independent original CPX instruction sites in the initial inventory scan.
        foreach (var (category, address) in new[] { new VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory.Beams, 0x82abcc), new VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory.Suits, 0x82abeb), new VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory.Boots, 0x82ac05) })
        {
            AssertEqual((byte)0xe0, rom.ReadCartridgeByte(address), "original inventory scan CPX");
            AssertEqual(ReadVerificationWord(rom, address + 1) / 2,
                PauseMenuSourceDefinitions.SelectorColumns.Single(column => column[0].Category == category).Length, "native category item-count bound");
        }
    }

    /// <summary>Each inventory column's category and its position in the cartridge's per-category tables.</summary>
    private static IEnumerable<VerifyPauseCategoryItemCountsCase> InventoryTables() =>
        PauseMenuSourceDefinitions.SelectorColumns.Select((column, table) => new VerifyPauseCategoryItemCountsCase(column[0].Category, table))
            .Where(entry => entry.First != PauseEquipmentCategory.Reserves);

    /// <summary>Matches the import's label strip widths to the native dispatched category copy lengths.</summary>
    private static void VerifyPauseCategoryCopyLengths(ISnesAddressSpace rom)
    {
        // The dispatched category owns this byte count even if movement changes the selection.
        foreach (var (category, address) in new[] { new VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory.Beams, 0x82afce), new VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory.Suits, 0x82b0c8), new VerifyPauseCategoryItemCountsCase(PauseEquipmentCategory.Boots, 0x82b156) })
        {
            AssertEqual((byte)0xa9, rom.ReadCartridgeByte(address), "original equipment copy-length LDA");
            AssertEqual(ReadVerificationWord(rom, address + 1) / 2,
                PauseEquipmentLabelKeys.WordCount(category), "import label width matches the native dispatched category copy length");
        }
    }
}
