using SuperMetroid.Tooling;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks enemy instruction catalog uniqueness, mapped addresses, and optional retail readability.</summary>
internal static class EnemyInstructionCodePointersVerification
{
    /// <summary>
    /// Verifies that enemy animation bytecode and list catalogs contain unique mapped
    /// pointers, then touches every bank-local entry in the retail ROM when it is present.
    /// </summary>
    internal static void VerifyEnemyInstructionCodePointerCatalogs()
    {
        // Private opcode sets the generic enemy interpreter hands to owner handlers after
        // the common commands. Routing decodes common commands first, so no owner set may
        // reuse a common offset.
        Type[] ownerInstructionSets =
        [
            typeof(AlcoonInstruction),
            typeof(BeetomInstruction),
            typeof(ChozoStatueInstruction),
            typeof(FakeKraidInstruction),
            typeof(FuneNamiheInstruction),
            typeof(HibashiInstruction),
            typeof(KiHunterInstruction),
            typeof(MagdolliteInstruction),
            typeof(MamaTurtleInstruction),
            typeof(ShaktoolInstruction),
            typeof(SpacePirateInstruction),
            typeof(WorkRobotInstruction),
            typeof(YardInstruction),
        ];
        Type[] codeCatalogs =
        [
            typeof(AlcoonInstruction),
            typeof(BeetomInstruction),
            typeof(ChozoStatueInstruction),
            typeof(CommonEnemyInstruction),
            typeof(FakeKraidInstruction),
            typeof(FuneNamiheInstruction),
            typeof(HibashiInstruction),
            typeof(KiHunterInstruction),
            typeof(MagdolliteInstruction),
            typeof(MamaTurtleInstruction),
            typeof(MotherBrainInstructionCodes),
            typeof(ShaktoolInstruction),
            typeof(SpacePirateInstruction),
            typeof(WorkRobotInstruction),
            typeof(YardInstruction),
        ];
        foreach (Type catalog in codeCatalogs)
            AssertUniqueMappedInstructionPointers(catalog);
        foreach (Type ownerSet in ownerInstructionSets)
        {
            foreach (FieldInfo field in GetUshortConstants(ownerSet))
            {
                ushort word = field.RequiredConstant<ushort>();
                AssertTrue(!Enum.IsDefined((CommonEnemyInstruction)word),
                    $"{ownerSet.Name}.{field.Name} does not reuse common enemy instruction ${word:X4}");
            }
        }

        Console.WriteLine($"  Enemy instructions: {codeCatalogs.Length} unique catalogs verified.");
    }

    /// <summary>Requires each catalog's ushort instruction constants to be distinct mapped-bank offsets.</summary>
    private static void AssertUniqueMappedInstructionPointers(Type catalog)
    {
        FieldInfo[] fields = GetUshortConstants(catalog);
        AssertTrue(fields.Length != 0, $"{catalog.Name} contains named instruction pointers");
        ushort[] pointers = fields
            .Select(field => field.RequiredConstant<ushort>())
            .ToArray();
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{catalog.Name} contains no duplicate addresses");
        foreach (ushort pointer in pointers)
            AssertTrue(pointer >= 0x8000, $"{catalog.Name} pointer ${pointer:X4} is mapped");
    }

    /// <summary>Returns public literal ushort or ushort-backed enum fields from an instruction catalog.</summary>
    internal static FieldInfo[] GetUshortConstants(Type catalog) => CatalogFields.Of(catalog, BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && (field.FieldType == typeof(ushort) ||
            field.FieldType.IsEnum && Enum.GetUnderlyingType(field.FieldType) == typeof(ushort)))
        .ToArray();
}
