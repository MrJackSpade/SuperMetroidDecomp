using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CatalogInventories;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the compiled enemy sprite-selector catalog covers the native inventory.</summary>
internal static class EnemyVisualSelectorInventoryVerification
{
    /// <summary>Collects bank-resolved selector addresses and reports any differences from the generated catalog.</summary>
    internal static void InspectEnemyVisualSelectors()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var (keyed, unresolved, owners) = EnemyVisualSelectorInventory.Collect(rom);
        Console.WriteLine($"Enemy visual selectors: {keyed.Count} unique bank-resolved addresses.");
        if (keyed.Count != CompiledEnemyVisualSelectorsTooling.Count)
        {
            for (int index = 0; index < CompiledEnemyVisualSelectorsTooling.Count; index++)
            {
                CompiledEnemyVisualSelector entry = CompiledEnemyVisualSelectorsTooling.At(index);
                if (!keyed.ContainsKey(entry.Address))
                    Console.WriteLine($"CATALOG-ONLY ${entry.Address:X6} -> ${entry.Pointer:X4}");
            }
            var cataloged = Enumerable.Range(0, CompiledEnemyVisualSelectorsTooling.Count)
                .Select(index => CompiledEnemyVisualSelectorsTooling.At(index).Address).ToHashSet();
            foreach ((int address, ushort pointer) in keyed.Where(pair => !cataloged.Contains(pair.Key)).OrderBy(pair => pair.Key))
                Console.WriteLine($"INVENTORY-ONLY ${address:X6} -> ${pointer:X4} ({owners[address]})");
        }
        AssertEqual(4364, keyed.Count, "distinct native sprite-selector addresses");
        AssertEqual(keyed.Count, CompiledEnemyVisualSelectorsTooling.Count,
            "generated catalog covers every bank-resolved inventory address");
        int catalogIndex = 0;
        foreach ((int address, ushort pointer) in keyed.OrderBy(pair => pair.Key))
        {
            CompiledEnemyVisualSelector entry =
                CompiledEnemyVisualSelectorsTooling.At(catalogIndex++);
            AssertEqual(address, entry.Address,
                "generated visual selector address matches catalog inventory");
            AssertEqual(pointer, entry.Pointer,
                "generated visual selector target matches catalog inventory");
        }
        foreach (string item in unresolved)
            Console.WriteLine($"UNRESOLVED {item}");
    }

    /// <summary>Checks every checked-in selector directly against the pinned cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled enemy visual selectors")]
    internal static void VerifyCompiledEnemyVisualSelectors()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(4249, CompiledEnemyVisualSelectorsTooling.Count,
            "generated fixed visual-selector count");
        foreach (int index in new[] { int.MinValue, -1, CompiledEnemyVisualSelectorsTooling.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CompiledEnemyVisualSelectorsTooling.At(index),
                "Shared selector enumeration bounds");
        int previousAddress = -1;
        for (int index = 0; index < CompiledEnemyVisualSelectorsTooling.Count; index++)
        {
            CompiledEnemyVisualSelector entry = CompiledEnemyVisualSelectorsTooling.At(index);
            AssertTrue(entry.Address > previousAddress,
                "generated visual selectors are distinct and sorted");
            previousAddress = entry.Address;
            ushort native = rom.ReadCartridgeWordFixedBank(entry.Address);
            AssertEqual(native, entry.Pointer,
                $"compiled visual selector ${entry.Address:X6}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(
                    SnesLittleEndian.BankByte(entry.Address),
                    unchecked((ushort)entry.Address), out ushort selected),
                $"compiled visual selector lookup ${entry.Address:X6}");
            AssertEqual(native, selected,
                $"compiled visual selector lookup target ${entry.Address:X6}");
        }
        AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x80, 0x8000, out _),
            "uncatalogued visual selector is not invented");
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.PaletteOperandCount;
             index++)
        {
            ushort address = CeresBabyInstructionProgramDefinitions
                .PaletteOperandAddress(index);
            AssertTrue(!CompiledEnemyVisualSelectors.TryGet(
                    CeresBabyInstructionProgramDefinitions.Bank, address, out _),
                $"Ceres Baby palette operand $A6:{address:X4} is not a sprite selector");
        }
        Console.WriteLine(
            $"Compiled enemy visuals: {CompiledEnemyVisualSelectorsTooling.Count:N0} distinct sprite selectors match the cartridge; " +
            "sorted lookup and unknown-key rejection pass.");
    }

    /// <summary>Runs the diagnostic native selector inventory comparison.</summary>
    [VerificationCommand("--enemy-visual-selector-inventory")]
    internal static int EnemyVisualSelectorInventoryCommand()
    {
        InspectEnemyVisualSelectors();
        return 0;
    }

    /// <summary>Runs the full compiled selector lookup and cartridge identity verification.</summary>
    [VerificationCommand("--verify-enemy-visual-selectors")]
    internal static int VerifyEnemyVisualSelectorsCommand()
    {
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
