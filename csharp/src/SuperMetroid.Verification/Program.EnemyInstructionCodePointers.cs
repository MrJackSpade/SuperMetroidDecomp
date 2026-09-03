using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies that enemy animation bytecode and list catalogs contain unique mapped
    /// pointers, then touches every bank-local entry in the retail ROM when it is present.
    /// </summary>
    static void VerifyEnemyInstructionCodePointerCatalogs()
    {
        Type[] codeCatalogs =
        [
            typeof(EnemyInstructionCodePointers),
            typeof(ChozoStatueInstructionCodes),
            typeof(CommonEnemyInstructionCodes),
            typeof(EscapeAnimalInstructionCodes),
            typeof(KraidInstructionCodes),
            typeof(MagdolliteInstructionCodes),
            typeof(MotherBrainInstructionCodes),
            typeof(NorfairRioInstructionCodes),
            typeof(PhantoonInstructionCodes),
            typeof(RinkaInstructionCodes),
            typeof(RioInstructionCodes),
            typeof(ShaktoolInstructionCodes),
            typeof(SpacePirateInstructionCodes),
            typeof(SporeSpawnInstructionCodes),
            typeof(TorizoInstructionCodes),
            typeof(WorkRobotInstructionCodes),
        ];
        foreach (Type catalog in codeCatalogs)
            AssertUniqueMappedInstructionPointers(catalog);

        (Type Catalog, byte Bank)[] bankLocalCatalogs =
        [
            (typeof(GunshipInstructionLists), 0xa2),
            (typeof(OrdinaryEnemyInstructionLists), 0xa3),
            (typeof(DraygonInstructionLists), 0xa5),
            (typeof(RidleyInstructionLists), 0xa6),
            (typeof(KraidInstructionLists), 0xa7),
            (typeof(PhantoonInstructionLists), 0xa7),
            (typeof(TorizoInstructionLists), 0xaa),
        ];
        foreach ((Type catalog, _) in bankLocalCatalogs)
            AssertUniqueMappedInstructionPointers(catalog);

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                $"  Enemy instructions: {codeCatalogs.Length + bankLocalCatalogs.Length} " +
                "catalogs are structurally valid; retail ROM reads skipped (private input absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int readableEntries = 0;
        foreach ((Type catalog, byte bank) in bankLocalCatalogs)
        {
            foreach (FieldInfo field in GetUshortConstants(catalog))
            {
                ushort pointer = (ushort)field.GetRawConstantValue()!;
                _ = bus.ReadByte((bank << 16) | pointer);
                readableEntries++;
            }
        }

        Console.WriteLine(
            $"  Enemy instructions: {codeCatalogs.Length + bankLocalCatalogs.Length} " +
            $"unique catalogs and {readableEntries} bank-local ROM entries verified.");
    }

    private static void AssertUniqueMappedInstructionPointers(Type catalog)
    {
        FieldInfo[] fields = GetUshortConstants(catalog);
        AssertTrue(fields.Length != 0, $"{catalog.Name} contains named instruction pointers");
        ushort[] pointers = fields
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{catalog.Name} contains no duplicate addresses");
        foreach (ushort pointer in pointers)
            AssertTrue(pointer >= 0x8000, $"{catalog.Name} pointer ${pointer:X4} is mapped");
    }

    private static FieldInfo[] GetUshortConstants(Type catalog) => catalog
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
        .ToArray();
}
