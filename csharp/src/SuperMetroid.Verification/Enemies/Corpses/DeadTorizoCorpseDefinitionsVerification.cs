using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Dead Torizo corpse metadata and its production initializer.</summary>
internal static class DeadTorizoCorpseDefinitionsVerification
{
    /// <summary>Compares the native configuration and wrap offset with the slot state created without ROM metadata reads.</summary>
    internal static void VerifyDeadTorizoCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;

        // Native configuration record $A9:DD58. Word 5 (graphics initializer $DE18) has
        // no port counterpart; the remaining seven words are the definition's fields.
        const int address = 0xa9dd58;
        ushort[] expected = new ushort[7];
        for (int index = 0, word = 0; word < 8; word++)
        {
            if (word == 5)
                continue;
            expected[index++] = CartridgeImportSource.Require(rom).ReadCartridgeWord((address + word * 2));
        }

        ushort[] actual =
        [
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.EntryCount,
            definition.RotationTablePointer,
            definition.FinishFunction,
        ];
        AssertTrue(actual.SequenceEqual(expected),
            "dead Torizo corpse configuration");

        ushort secondRotationOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2))));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            "dead Torizo corpse wrap offset");
        AssertEqual((ushort)CorpseRotationTable.Torizo, definition.RotationTablePointer,
            "dead Torizo configuration names the Torizo tile-row layout production copies with");
        AssertEqual((ushort)0, definition.VramTransferPointer,
            "dead Torizo has no generic transfer list; it uploads through its alternating halves");

        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = enemies.DeadTorizoBehavior.InitializeDeadTorizo;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        initialize(slot);

        DeadTorizoEnemyState state = (enemies.DeadTorizo ?? throw new InvalidOperationException("Verification requires enemies.DeadTorizo."));
        CorpseRotTableChecks.AssertConfiguredRotTable(enemies, definition.RottingTablePointer, definition.EntryCount,
            (state ?? throw new InvalidOperationException("Verification requires state.")).RotTable, "dead Torizo production");
        AssertEqual(definition.EntryCount - 1, state.YLimit,
            "dead Torizo production Y limit");
        AssertEqual(definition.EntryCount - 2, state.LateMoveEntryIndex,
            "dead Torizo production late-move row");
        AssertEqual(definition.WrapOffset, state.WrapOffset,
            "dead Torizo production wrap offset");

        Console.WriteLine(
            "Dead Torizo corpse definitions: all eight native configuration words, the derived wrap offset, and the real initializer pass with migrated metadata reads forbidden.");
    }


    /// <summary>Runs the Dead Torizo corpse metadata and initializer checks.</summary>
    [VerificationCommand("--dead-torizo-corpse-definitions")]
    internal static int DeadTorizoCorpseDefinitionsCommand()
    {
        Suite(nameof(VerifyDeadTorizoCorpseDefinitions), () => VerifyDeadTorizoCorpseDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
