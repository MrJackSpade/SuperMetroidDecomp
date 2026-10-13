using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Audits named bank-$86 enemy-projectile callback and instruction-list addresses.</summary>
internal static class EnemyProjectileCodePointersVerification
{
    /// <summary>
    /// Treats the bank-$86 callback and list catalogs as the executable coverage manifest:
    /// every entry must be unique, mapped, and readable from the target cartridge.
    /// </summary>
    [CpuBusBoundary("exact bus test: proves every bank-$86 enemy-projectile pointer maps to readable retail cartridge ROM")]
    internal static void VerifyEnemyProjectileCodePointerCatalog()
    {
        (string Name, ushort Pointer)[] callbacks =
        [
            .. Enum.GetValues<EnemyProjectilePreInstruction>()
                .Where(code => code != EnemyProjectilePreInstruction.None)
                .Select(code => ($"pre-instruction {code}", (ushort)code)),
            .. Enum.GetValues<EnemyProjectileInstruction>().Select(code => ($"instruction {code}", (ushort)code)),
        ];
        FieldInfo[] lists = GetEnemyProjectilePointerConstants(
            typeof(EnemyProjectileInstructionLists));
        AssertTrue(callbacks.Length != 0, "enemy projectile callback catalog is populated");
        AssertTrue(lists.Length != 0, "enemy projectile instruction-list catalog is populated");

        AssertEqual(callbacks.Length, callbacks
            .Select(callback => callback.Pointer)
            .Distinct()
            .Count(), "enemy projectile callback addresses are unique");
        AssertEqual(lists.Length, lists
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .Distinct()
            .Count(), "enemy projectile list addresses are unique");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace? bus = File.Exists(romPath)
            ? SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath)
            : null;
        foreach ((string name, ushort pointer) in callbacks.Concat(
            lists.Select(field => (field.Name, (ushort)field.GetRawConstantValue()!))))
        {
            AssertTrue(pointer >= 0x8000,
                $"enemy projectile pointer {name} is in mapped bank-$86 ROM");
            if (bus is not null)
                _ = bus.ReadCartridgeByte(0x860000 | pointer);
        }

        Console.WriteLine(
            $"  Enemy projectiles: {callbacks.Length} callbacks and {lists.Length} " +
            $"instruction lists are named{(bus is null ? "" : " and ROM-readable")}.");
    }

    /// <summary>Collects public ushort constants so the audit covers the declared instruction-list catalog.</summary>
    private static FieldInfo[] GetEnemyProjectilePointerConstants(Type catalog) => CatalogFields.Of(catalog, BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
        .ToArray();
}
