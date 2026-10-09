using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks pause equipment category metadata against cartridge pointers, bounds, copy lengths, and index behavior.</summary>
    /// <param name="rom">Address space containing the native pause-menu category tables and instructions.</param>
    private static void VerifyPauseCategoryCases(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyPauseCategoryOffsets), () => VerifyPauseCategoryOffsets(rom));
        Suite(nameof(VerifyPauseCategoryTilemapPointers), () => VerifyPauseCategoryTilemapPointers(rom));
        Suite(nameof(VerifyPauseCategoryItemCounts), () => VerifyPauseCategoryItemCounts(rom));
        Suite(nameof(VerifyPauseCategoryCopyLengths), () => VerifyPauseCategoryCopyLengths(rom));
        AssertEqual(new PauseEquipmentCategoryDefinition(0, 0, 0, 0, 0),
            PauseEquipmentCategories.Get(0), "reserve controls retain the original managed zero-data contract");
        for (int category = 0; category < 4; category++)
            AssertEqual(category, PauseEquipmentCategories.Get(category).Category, "category identity");
        foreach (int invalid in new[] { int.MinValue, -1, 4, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PauseEquipmentCategories.Get(invalid),
                "invalid pause category preserves former array boundary");
    }

    /// <summary>Verifies each non-reserve category's tilemap-offset table address against its bank-$82 pointer.</summary>
    /// <param name="rom">Address space containing the category pointer table.</param>
    private static void VerifyPauseCategoryOffsets(ISnesAddressSpace rom)
    {
        for (int category = 1; category <= 3; category++)
            AssertEqual(0x820000 | ReadVerificationWord(rom, 0x82c02c + 2 * category),
                PauseEquipmentCategories.Get(category).OffsetTableAddress, "native category tilemap-offset pointer");
    }

    /// <summary>Verifies each non-reserve category's tilemap-list pointer against the native pointer table.</summary>
    /// <param name="rom">Address space containing the category tilemap-list pointers.</param>
    private static void VerifyPauseCategoryTilemapPointers(ISnesAddressSpace rom)
    {
        for (int category = 1; category <= 3; category++)
            AssertEqual(0x820000 | ReadVerificationWord(rom, 0x82c044 + 2 * category),
                PauseEquipmentCategories.Get(category).TilemapPointerTableAddress, "native category tilemap-list pointer");
    }

    /// <summary>Checks category item-count bounds against the immediate operands in the cartridge inventory scan.</summary>
    /// <param name="rom">Address space containing the original inventory-scan instructions.</param>
    private static void VerifyPauseCategoryItemCounts(ISnesAddressSpace rom)
    {
        // Independent original CPX instruction sites in the initial inventory scan.
        foreach (var (category, address) in new[] { (1, 0x82abcc), (2, 0x82abeb), (3, 0x82ac05) })
        {
            AssertEqual((byte)0xe0, rom.ReadByte(address), "original inventory scan CPX");
            AssertEqual(ReadVerificationWord(rom, address + 1) / 2,
                PauseEquipmentCategories.Get(category).ItemCount, "native category item-count bound");
        }
    }

    /// <summary>Checks label-copy lengths against the category-dispatch instruction operands.</summary>
    /// <param name="rom">Address space containing the original equipment-copy instructions.</param>
    private static void VerifyPauseCategoryCopyLengths(ISnesAddressSpace rom)
    {
        // The dispatched category owns this byte count even if movement changes the selection.
        foreach (var (category, address) in new[] { (1, 0x82afce), (2, 0x82b0c8), (3, 0x82b156) })
        {
            AssertEqual((byte)0xa9, rom.ReadByte(address), "original equipment copy-length LDA");
            AssertEqual(ReadVerificationWord(rom, address + 1) / 2,
                PauseEquipmentCategories.Get(category).LabelWordCount, "native dispatched category label length");
        }
    }
}
