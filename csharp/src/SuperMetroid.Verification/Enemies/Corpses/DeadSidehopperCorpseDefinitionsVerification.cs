using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks alive and dead Sidehopper corpse metadata and production initialization.</summary>
internal static class DeadSidehopperCorpseDefinitionsVerification
{
    /// <summary>Configuration word indices with a compiled consumer; word five, the graphics-initialization callback, has none.</summary>
    private static readonly int[] ConsumedConfigurationWords =
        [0, 1, 4];

    /// <summary>Compares both native configuration records and initialized rotting state without metadata rereads.</summary>
    internal static void VerifyDeadSidehopperCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyAlive,
            configurationPointer: 0xdd68));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyDead,
            configurationPointer: 0xdd78));

        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            parameter1: 0,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive));
        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            parameter1: 2,
            graphicsVariant: 2,
            DeadSidehopperCorpseDefinitions.InitiallyDead));

        Console.WriteLine(
            "Dead sidehopper corpse definitions: all 14 consumed native configuration words, two derived wrap offsets, and both production initializers pass with migrated metadata reads forbidden.");
    }

    /// <summary>Compares the consumed words and rotation-derived wrap offset of one corpse record.</summary>
    private static void VerifyDefinition(
        SuperMetroidAddressSpace rom,
        DeadSidehopperCorpseDefinition definition,
        ushort configurationPointer)
    {
        int address = 0xa90000 | configurationPointer;
        ushort Word(int index) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address + index * 2);
        ushort[] actual = [definition.RottingTablePointer, definition.VramTransferPointer, definition.EntryCount];
        AssertTrue(actual.SequenceEqual(ConsumedConfigurationWords.Select(Word)),
            $"dead sidehopper configuration $A9:{configurationPointer:X4}");

        ushort rotationTable = Word(6);
        ushort secondRotationOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | unchecked((ushort)(rotationTable + 2))));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            $"dead sidehopper configuration $A9:{configurationPointer:X4} wrap offset");
        AssertEqual((ushort)CorpseRotationTable.Sidehopper, rotationTable,
            $"dead sidehopper configuration $A9:{configurationPointer:X4} names the sidehopper tile-row layout production copies with");
    }

    /// <summary>Checks the state installed for one alive or initially dead enemy parameter.</summary>
    private static void VerifyProductionInitialization(
        ushort parameter1,
        ushort graphicsVariant,
        DeadSidehopperCorpseDefinition expected)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        Action<RoomEnemySlot> initialize = enemies.DeadSidehopperBehavior.InitializeDeadSidehopper;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.CorpseSidehopper, slot.Definition);
        slot.SetSpawnParameter1(parameter1);

        initialize(slot);

        DeadSidehopperEnemyState state = (enemies.DeadSidehoppers[0] ?? throw new InvalidOperationException("Verification requires enemies.DeadSidehoppers[0]."));
        AssertEqual(graphicsVariant, state.GraphicsVariant,
            $"dead sidehopper parameter {parameter1} graphics variant");
        CorpseRotTableChecks.AssertConfiguredRotTable(enemies, expected.RotTableOffset, expected.EntryCount,
            state.RotTable, $"dead sidehopper parameter {parameter1}");
        AssertEqual(expected.VramTransfers, state.Flesh,
            $"dead sidehopper parameter {parameter1} uploads the configured VRAM transfer list");
        AssertEqual(expected.EntryCount - 1, state.YLimit,
            $"dead sidehopper parameter {parameter1} Y limit");
        AssertEqual(expected.EntryCount - 2, state.LateMoveEntryIndex,
            $"dead sidehopper parameter {parameter1} late-move row");
        AssertEqual(expected.WrapOffset, state.WrapOffset,
            $"dead sidehopper parameter {parameter1} wrap offset");
    }


    /// <summary>Runs the Sidehopper corpse definition check with the retail cartridge.</summary>
    [VerificationCommand("--dead-sidehopper-corpse-definitions")]
    internal static int DeadSidehopperCorpseDefinitionsCommand()
    {
        Suite(nameof(VerifyDeadSidehopperCorpseDefinitions), () => VerifyDeadSidehopperCorpseDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
