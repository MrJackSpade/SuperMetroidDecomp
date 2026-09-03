using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Treats the bank-$86 callback and list catalogs as the executable coverage manifest:
    /// every entry must be unique, mapped, and readable from the target cartridge.
    /// </summary>
    static void VerifyEnemyProjectileCodePointerCatalog()
    {
        FieldInfo[] callbacks = GetEnemyProjectilePointerConstants(
            typeof(EnemyProjectileCodePointers));
        FieldInfo[] lists = GetEnemyProjectilePointerConstants(
            typeof(EnemyProjectileInstructionLists));
        AssertTrue(callbacks.Length != 0, "enemy projectile callback catalog is populated");
        AssertTrue(lists.Length != 0, "enemy projectile instruction-list catalog is populated");

        AssertEqual(callbacks.Length, callbacks
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .Distinct()
            .Count(), "enemy projectile callback addresses are unique");
        AssertEqual(lists.Length, lists
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .Distinct()
            .Count(), "enemy projectile list addresses are unique");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace? bus = File.Exists(romPath)
            ? SuperMetroidAddressSpace.LoadRetailRom(romPath)
            : null;
        foreach (FieldInfo field in callbacks.Concat(lists))
        {
            ushort pointer = (ushort)field.GetRawConstantValue()!;
            AssertTrue(pointer >= 0x8000,
                $"enemy projectile pointer {field.Name} is in mapped bank-$86 ROM");
            if (bus is not null)
                _ = bus.ReadByte(0x860000 | pointer);
        }

        Console.WriteLine(
            $"  Enemy projectiles: {callbacks.Length} callbacks and {lists.Length} " +
            $"instruction lists are named{(bus is null ? "" : " and ROM-readable")}.");
    }

    private static FieldInfo[] GetEnemyProjectilePointerConstants(Type catalog) => catalog
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
        .ToArray();
}
