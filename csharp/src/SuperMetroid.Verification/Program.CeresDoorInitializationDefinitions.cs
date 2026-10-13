using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

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
            AssertEqual((ushort)1, slot.InstructionTimer,
                $"production Ceres door instruction timer {variant}");
            AssertEqual((ushort)0, slot.Timer,
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
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
