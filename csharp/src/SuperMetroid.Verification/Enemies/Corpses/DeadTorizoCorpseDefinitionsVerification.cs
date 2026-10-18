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

        // Native configuration record $A9:DD58: rotting table, entry count and the rotation table the wrap offset
        // derives from are the words production consumes.
        const int address = 0xa9dd58;
        ushort Word(int index) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address + index * 2);
        AssertEqual(Word(0), definition.RottingTablePointer, "dead Torizo corpse rotting table");
        AssertEqual(Word(4), definition.EntryCount, "dead Torizo corpse entry count");

        ushort rotationTable = Word(6);
        ushort secondRotationOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | unchecked((ushort)(rotationTable + 2))));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            "dead Torizo corpse wrap offset");
        AssertEqual((ushort)CorpseRotationTable.Torizo, rotationTable,
            "dead Torizo configuration names the Torizo tile-row layout production copies with");
        AssertEqual((ushort)0, Word(1),
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
