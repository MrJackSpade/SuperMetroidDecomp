using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks alive and dead Sidehopper corpse metadata and production initialization.</summary>
internal static class DeadSidehopperCorpseDefinitionsVerification
{
    /// <summary>Compares both native configuration records and initialized rotting state without metadata rereads.</summary>
    internal static void VerifyDeadSidehopperCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyAlive,
            expectedConfigurationPointer: 0xdd68));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyDead,
            expectedConfigurationPointer: 0xdd78));

        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            rom,
            parameter1: 0,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive));
        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            rom,
            parameter1: 2,
            graphicsVariant: 2,
            DeadSidehopperCorpseDefinitions.InitiallyDead));

        Console.WriteLine(
            "Dead sidehopper corpse definitions: all 14 consumed native configuration words, two derived wrap offsets, and both production initializers pass with migrated metadata reads forbidden.");
    }

    /// <summary>Compares the seven consumed words and rotation-derived wrap offset of one corpse record.</summary>
    private static void VerifyDefinition(
        SuperMetroidAddressSpace rom,
        DeadSidehopperCorpseDefinition definition,
        ushort expectedConfigurationPointer)
    {
        AssertEqual(expectedConfigurationPointer, definition.ConfigurationPointer,
            "dead sidehopper configuration identity");
        int address = 0xa90000 | definition.ConfigurationPointer;
        // Word five, the graphics-initialization callback, has no compiled consumer.
        ushort[] expected = [.. new[] { 0, 1, 2, 3, 4, 6, 7 }
            .Select(index => CartridgeImportSource.Require(rom).ReadCartridgeWord((address + index * 2)))];

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
            $"dead sidehopper configuration $A9:{definition.ConfigurationPointer:X4}");

        ushort secondRotationOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2))));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            $"dead sidehopper configuration $A9:{definition.ConfigurationPointer:X4} wrap offset");
    }

    /// <summary>Checks the state installed for one alive or initially dead enemy parameter.</summary>
    private static void VerifyProductionInitialization(
        SuperMetroidAddressSpace rom,
        ushort parameter1,
        ushort graphicsVariant,
        DeadSidehopperCorpseDefinition expected)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureBus(NewDeadSidehopperCorpseDefinitionReadGuard(rom));
        Action<RoomEnemySlot> initialize = enemies.DeadSidehopperBehavior.InitializeDeadSidehopper;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.CorpseSidehopper, slot.Definition);
        slot.Parameter1 = parameter1;

        initialize(slot);

        DeadSidehopperEnemyState state = (enemies.DeadSidehoppers[0] ?? throw new InvalidOperationException("Verification requires enemies.DeadSidehoppers[0]."));
        AssertEqual(graphicsVariant, (state ?? throw new InvalidOperationException("Verification requires state.")).GraphicsVariant,
            $"dead sidehopper parameter {parameter1} graphics variant");
        AssertEqual(expected.ConfigurationPointer, state.ConfigurationPointer,
            $"dead sidehopper parameter {parameter1} configuration");
        AssertEqual(expected.RottingTablePointer, state.TablePointer,
            $"dead sidehopper parameter {parameter1} rotting table");
        AssertEqual(expected.VramTransferPointer, state.VramTablePointer,
            $"dead sidehopper parameter {parameter1} VRAM table");
        AssertEqual(expected.CopyFunction, state.CopyFunction,
            $"dead sidehopper parameter {parameter1} copy callback");
        AssertEqual(expected.MoveFunction, state.MoveFunction,
            $"dead sidehopper parameter {parameter1} move callback");
        AssertEqual(expected.RotationTablePointer, state.RotationTablePointer,
            $"dead sidehopper parameter {parameter1} rotation table");
        AssertEqual(expected.FinishFunction, state.FinishFunction,
            $"dead sidehopper parameter {parameter1} finish callback");
        AssertEqual(expected.EntryCount, state.EntryCount,
            $"dead sidehopper parameter {parameter1} row count");
        AssertEqual(unchecked((ushort)(expected.EntryCount - 1)), state.YLimit,
            $"dead sidehopper parameter {parameter1} Y limit");
        AssertEqual(unchecked((ushort)(expected.EntryCount - 2)), state.LateMoveEntryIndex,
            $"dead sidehopper parameter {parameter1} late-move row");
        AssertEqual(expected.WrapOffset, state.WrapOffset,
            $"dead sidehopper parameter {parameter1} wrap offset");
    }

    /// <summary>Blocks the migrated corpse records and rotation offset during initialization.</summary>
    private static CartridgeReadGuard NewDeadSidehopperCorpseDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa9dd68 and < 0xa9dd88 or >= 0xa9e242 and < 0xa9e244,
                address => $"Dead sidehopper attempted migrated corpse metadata read ${address:X6}.");

    /// <summary>Runs the Sidehopper corpse definition check with the retail cartridge.</summary>
    [VerificationCommand("--dead-sidehopper-corpse-definitions")]
    internal static int DeadSidehopperCorpseDefinitionsCommand()
    {
        Suite(nameof(VerifyDeadSidehopperCorpseDefinitions), () => VerifyDeadSidehopperCorpseDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
