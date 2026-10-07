using System.Reflection;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>The bank-resolved selector inventory matches the generated catalog address for address.</summary>
    private static void InspectEnemyVisualSelectors()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var (discovered, catalogs, operands, ordinary, special, keyed, families, unresolved) =
            EnemyVisualSelectorInventory.Collect(rom);
        Console.WriteLine(
            $"Discovered={discovered}, catalogs={catalogs}, " +
            $"mapped families={families.Count}, " +
            $"operands={operands}, unique addresses={keyed.Count}, " +
            $"plausible OAM={ordinary}, nonstandard={special}.");
        if (keyed.Count != CompiledEnemyVisualSelectors.Count)
        {
            for (int index = 0; index < CompiledEnemyVisualSelectors.Count; index++)
            {
                CompiledEnemyVisualSelector entry = CompiledEnemyVisualSelectors.At(index);
                if (!keyed.ContainsKey(entry.Address))
                    Console.WriteLine($"CATALOG-ONLY ${entry.Address:X6} -> ${entry.Pointer:X4}");
            }
        }
        AssertEqual(156, discovered, "instruction catalogs with visual operands");
        AssertEqual(5247, operands, "counted native sprite-selector occurrences");
        AssertEqual(5102, keyed.Count, "distinct native sprite-selector addresses");
        AssertEqual(keyed.Count, CompiledEnemyVisualSelectors.Count,
            "generated catalog covers every bank-resolved inventory address");
        int catalogIndex = 0;
        foreach ((int address, ushort pointer) in keyed.OrderBy(pair => pair.Key))
        {
            CompiledEnemyVisualSelector entry =
                CompiledEnemyVisualSelectors.At(catalogIndex++);
            AssertEqual(address, entry.Address,
                "generated visual selector address matches catalog inventory");
            AssertEqual(pointer, entry.Pointer,
                "generated visual selector target matches catalog inventory");
        }
        foreach ((string name, int frameCount, int specialCount) in families
                     .OrderByDescending(family => family.Ordinary)
                     .ThenBy(family => family.Name, StringComparer.Ordinal))
            Console.WriteLine($"{name}: ordinary={frameCount}, special={specialCount}");
        foreach (string item in unresolved)
            Console.WriteLine($"UNRESOLVED {item}");

    }

    /// <summary>Checks every checked-in selector directly against the pinned cartridge.</summary>
    private static void VerifyCompiledEnemyVisualSelectors()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(5102, CompiledEnemyVisualSelectors.Count,
            "generated fixed visual-selector count");
        foreach (int index in new[] { int.MinValue, -1, CompiledEnemyVisualSelectors.Count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CompiledEnemyVisualSelectors.At(index),
                "Shared selector enumeration bounds");
        int previousAddress = -1;
        for (int index = 0; index < CompiledEnemyVisualSelectors.Count; index++)
        {
            CompiledEnemyVisualSelector entry = CompiledEnemyVisualSelectors.At(index);
            AssertTrue(entry.Address > previousAddress,
                "generated visual selectors are distinct and sorted");
            previousAddress = entry.Address;
            ushort native = (ushort)(rom.ReadByte(entry.Address) |
                rom.ReadByte((entry.Address & 0xff0000) |
                    unchecked((ushort)(entry.Address + 1))) << 8);
            AssertEqual(native, entry.Pointer,
                $"compiled visual selector ${entry.Address:X6}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(
                    unchecked((byte)(entry.Address >> 16)),
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
            $"Compiled enemy visuals: {CompiledEnemyVisualSelectors.Count:N0} distinct sprite selectors match the cartridge; " +
            "sorted lookup and unknown-key rejection pass.");
    }
}
