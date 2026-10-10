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
        // Private opcode sets the generic enemy interpreter hands to owner handlers after
        // the common commands. Routing decodes common commands first, so no owner set may
        // reuse a common offset.
        Type[] ownerInstructionSets =
        [
            typeof(AlcoonInstruction),
            typeof(BabyMetroidInstruction),
            typeof(BeetomInstruction),
            typeof(BotwoonInstruction),
            typeof(BoyonInstruction),
            typeof(CacatacInstruction),
            typeof(CeresDoorInstruction),
            typeof(CeresSteamInstruction),
            typeof(ChozoStatueInstruction),
            typeof(CrawlerInstruction),
            typeof(CrocomireInstruction),
            typeof(DeadSidehopperInstruction),
            typeof(DragonInstruction),
            typeof(DraygonInstruction),
            typeof(EscapeEtecoonInstruction),
            typeof(EscapeDachoraInstruction),
            typeof(EvirInstruction),
            typeof(FakeKraidInstruction),
            typeof(FuneNamiheInstruction),
            typeof(HibashiInstruction),
            typeof(HopperInstruction),
            typeof(HZoomerInstruction),
            typeof(KiHunterInstruction),
            typeof(KraidArmInstruction),
            typeof(KraidFootInstruction),
            typeof(LowerNorfairRioInstruction),
            typeof(MagdolliteInstruction),
            typeof(MamaTurtleInstruction),
            typeof(MaridiaLargeSnailInstruction),
            typeof(MetareeInstruction),
            typeof(MetroidInstruction),
            typeof(MotherBrainInstruction),
            typeof(NorfairRioInstruction),
            typeof(OwtchInstruction),
            typeof(PhantoonPartInstruction),
            typeof(PlatformInstruction),
            typeof(RidleyInstruction),
            typeof(RinkaInstruction),
            typeof(ShaktoolInstruction),
            typeof(ShitroidInstruction),
            typeof(SkreeInstruction),
            typeof(SkulteraInstruction),
            typeof(SpacePirateInstruction),
            typeof(SparkInstruction),
            typeof(SporeSpawnInstruction),
            typeof(StokeInstruction),
            typeof(TorizoInstruction),
            typeof(WaverInstruction),
            typeof(WorkRobotInstruction),
            typeof(YappingMawInstruction),
            typeof(YardInstruction),
            typeof(ZoaInstruction),
        ];
        Type[] codeCatalogs =
        [
            typeof(AlcoonInstruction),
            typeof(BeetomInstruction),
            typeof(BoyonInstruction),
            typeof(CacatacInstruction),
            typeof(CeresDoorInstruction),
            typeof(CeresSteamInstruction),
            typeof(ChozoStatueInstruction),
            typeof(CommonEnemyInstruction),
            typeof(CrawlerInstruction),
            typeof(DeadSidehopperInstruction),
            typeof(DragonInstruction),
            typeof(EscapeDachoraInstruction),
            typeof(EscapeEtecoonInstruction),
            typeof(EvirInstruction),
            typeof(FakeKraidInstruction),
            typeof(FuneNamiheInstruction),
            typeof(HibashiInstruction),
            typeof(HopperInstruction),
            typeof(HZoomerInstruction),
            typeof(KiHunterInstruction),
            typeof(KraidArmInstruction),
            typeof(KraidFootInstruction),
            typeof(MagdolliteInstruction),
            typeof(MamaTurtleInstruction),
            typeof(MetareeInstruction),
            typeof(MetroidInstruction),
            typeof(MotherBrainInstructionCodes),
            typeof(NorfairRioInstruction),
            typeof(OwtchInstruction),
            typeof(PhantoonInstruction),
            typeof(PhantoonPartInstruction),
            typeof(PlatformInstruction),
            typeof(RidleyInstruction),
            typeof(RinkaInstruction),
            typeof(RioInstructionCodes),
            typeof(ShaktoolInstruction),
            typeof(ShitroidInstruction),
            typeof(SkreeInstruction),
            typeof(SkulteraInstruction),
            typeof(SpacePirateInstruction),
            typeof(SparkInstruction),
            typeof(SporeSpawnInstruction),
            typeof(StokeInstruction),
            typeof(TorizoInstruction),
            typeof(WaverInstruction),
            typeof(WorkRobotInstruction),
            typeof(YappingMawInstruction),
            typeof(YardInstruction),
            typeof(ZoaInstruction),
        ];
        foreach (Type catalog in codeCatalogs)
            AssertUniqueMappedInstructionPointers(catalog);
        foreach (Type ownerSet in ownerInstructionSets)
        {
            foreach (FieldInfo field in GetUshortConstants(ownerSet))
            {
                ushort word = (ushort)field.GetRawConstantValue()!;
                AssertTrue(!Enum.IsDefined((CommonEnemyInstruction)word),
                    $"{ownerSet.Name}.{field.Name} does not reuse common enemy instruction ${word:X4}");
            }
        }

        (Type Catalog, byte Bank)[] bankLocalCatalogs =
        [
            (typeof(GunshipInstructionProgramDefinitions), 0xa2),
            (typeof(OrdinaryEnemyInstructionLists), 0xa3),
            (typeof(DraygonInstructionProgramDefinitions), 0xa5),
            (typeof(RidleyInstructionProgramDefinitions), 0xa6),
            (typeof(KraidLintInstructionLists), 0xa7),
            (typeof(PhantoonInstructionProgramDefinitions), 0xa7),
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

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
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

    private static FieldInfo[] GetUshortConstants(Type catalog) => CatalogFields.Of(catalog, BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && (field.FieldType == typeof(ushort) ||
            field.FieldType.IsEnum && Enum.GetUnderlyingType(field.FieldType) == typeof(ushort)))
        .ToArray();
}
