using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPauseCategoryCases(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyPauseCategoryOffsets), () => VerifyPauseCategoryOffsets(rom));
        Suite(nameof(VerifyPauseCategoryTilemapPointers), () => VerifyPauseCategoryTilemapPointers(rom));
        Suite(nameof(VerifyPauseCategoryItemCounts), () => VerifyPauseCategoryItemCounts(rom));
        Suite(nameof(VerifyPauseCategoryCopyLengths), () => VerifyPauseCategoryCopyLengths(rom));
        AssertEqual(new PauseEquipmentCategoryDefinition(0, 0, 0, 0, 0),
            PauseEquipmentCategories.Get(PauseEquipmentCategory.Reserves), "reserve controls retain the original managed zero-data contract");
        foreach (PauseEquipmentCategory category in Enum.GetValues<PauseEquipmentCategory>())
            AssertEqual(category, PauseEquipmentCategories.Get(category).Category, "category identity");
        foreach (PauseEquipmentCategory invalid in new[] { (PauseEquipmentCategory)4, (PauseEquipmentCategory)byte.MaxValue })
            AssertThrows<InvalidOperationException>(() => PauseEquipmentCategories.Get(invalid),
                "undefined pause category is rejected at the typed boundary");
    }

    private static void VerifyPauseCategoryOffsets(ISnesAddressSpace rom)
    {
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
            AssertEqual(0x820000 | ReadVerificationWord(rom, 0x82c02c + 2 * (int)category),
                PauseEquipmentCategories.Get(category).OffsetTableAddress, "native category tilemap-offset pointer");
    }

    private static void VerifyPauseCategoryTilemapPointers(ISnesAddressSpace rom)
    {
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
            AssertEqual(0x820000 | ReadVerificationWord(rom, 0x82c044 + 2 * (int)category),
                PauseEquipmentCategories.Get(category).TilemapPointerTableAddress, "native category tilemap-list pointer");
    }

    private static void VerifyPauseCategoryItemCounts(ISnesAddressSpace rom)
    {
        // Independent original CPX instruction sites in the initial inventory scan.
        foreach (var (category, address) in new[] { (PauseEquipmentCategory.Beams, 0x82abcc), (PauseEquipmentCategory.Suits, 0x82abeb), (PauseEquipmentCategory.Boots, 0x82ac05) })
        {
            AssertEqual((byte)0xe0, rom.ReadByte(address), "original inventory scan CPX");
            AssertEqual(ReadVerificationWord(rom, address + 1) / 2,
                PauseEquipmentCategories.Get(category).ItemCount, "native category item-count bound");
        }
    }

    private static void VerifyPauseCategoryCopyLengths(ISnesAddressSpace rom)
    {
        // The dispatched category owns this byte count even if movement changes the selection.
        foreach (var (category, address) in new[] { (PauseEquipmentCategory.Beams, 0x82afce), (PauseEquipmentCategory.Suits, 0x82b0c8), (PauseEquipmentCategory.Boots, 0x82b156) })
        {
            AssertEqual((byte)0xa9, rom.ReadByte(address), "original equipment copy-length LDA");
            AssertEqual(ReadVerificationWord(rom, address + 1) / 2,
                PauseEquipmentCategories.Get(category).LabelWordCount, "native dispatched category label length");
        }
    }
}
