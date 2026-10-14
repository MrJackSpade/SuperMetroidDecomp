using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;

/// <summary>Checks seven Ceres door initializer variants against their native selectors.</summary>
internal static class CeresDoorInitializationDefinitionsVerification
{
    /// <summary>Compares function and instruction words with live slot initialization under a ROM read guard.</summary>
    internal static void VerifyCeresDoorInitializationDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa6f52c;
        const int functionTable = 0xa6f72b;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var guarded = NewCeresDoorInitializationReadGuard(rom);

        for (ushort variant = 0; variant < 7; variant++)
        {
            CeresDoorInitializationDefinition definition =
                CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((functionTable + variant * 2)),
                (ushort)definition.MainFunction,
                $"Ceres door function selector {variant}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + variant * 2)),
                definition.InstructionList,
                $"Ceres door instruction selector {variant}");

            var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            enemies.BindFixtureBus(guarded);
            enemies.Presentation._vram = new SnesVram();
            enemies.Presentation._cgram = new SnesCgram();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = variant;

            enemies.CeresDoorBehavior.InitializeCeresDoor(slot);

            AssertEqual((ushort)definition.MainFunction, slot.VariableA,
                $"production Ceres door function selector {variant}");
            AssertEqual(definition.InstructionList, slot.CurrentInstruction,
                $"production Ceres door instruction selector {variant}");
            AssertEqual((ushort)1, slot.InstructionTimer.RawValue,
                $"production Ceres door instruction timer {variant}");
            AssertEqual((ushort)0, slot.Timer.RawValue,
                $"production Ceres door general timer {variant}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDoorInitializationDefinitions.For(7),
            "Ceres door selector past definitions");
        Console.WriteLine(
            "Ceres door initialization definitions: all fourteen native selector words and seven production initializers pass with both source tables forbidden.");
    }

    /// <summary>Rejects live rereads of both migrated Ceres door selector tables.</summary>
    private static CartridgeReadGuard NewCeresDoorInitializationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6f52c and < 0xa6f53a or >= 0xa6f72b and < 0xa6f739,
                address => $"Ceres door attempted migrated initialization-table read ${address:X6}.");

    /// <summary>Runs the Ceres door initialization selector checks.</summary>
    [VerificationCommand("--ceres-door-initialization-definitions")]
    internal static int CeresDoorInitializationDefinitionsCommand()
    {
        Suite(nameof(VerifyCeresDoorInitializationDefinitions), () => VerifyCeresDoorInitializationDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
