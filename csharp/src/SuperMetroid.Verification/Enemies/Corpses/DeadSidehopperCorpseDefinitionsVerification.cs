using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks alive and dead Sidehopper corpse metadata and production initialization.</summary>
internal static class DeadSidehopperCorpseDefinitionsVerification
{
    /// <summary>Compares both native configuration records and initialized rotting state without metadata rereads.</summary>
    internal static void VerifyDeadSidehopperCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyAlive,
            configurationPointer: 0xdd68,
            graphicsVariant: 0));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyDead,
            configurationPointer: 0xdd78,
            graphicsVariant: 2));

        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            parameter1: 0,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive));
        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            parameter1: 2,
            graphicsVariant: 2,
            DeadSidehopperCorpseDefinitions.InitiallyDead));

        Console.WriteLine(
            "Dead sidehopper corpse definitions: both decoded configuration records' kinds, heights, row layouts and wraps, and both production initializers' own fresh rot schedules pass.");
    }

    /// <summary>
    /// Compares one corpse record, decoded by the codec, with the typed definition and the extraction's row movers: the
    /// kind whose upload list it names, the corpse's height, its row layout and its wrap.
    /// </summary>
    private static void VerifyDefinition(
        SuperMetroidAddressSpace rom,
        DeadSidehopperCorpseDefinition definition,
        ushort configurationPointer,
        int graphicsVariant)
    {
        CorpseRottingConfiguration configuration = CorpseRottingConfigurationCodec.Decode(rom, configurationPointer);
        string context = $"dead sidehopper configuration $A9:{configurationPointer:X4}";
        AssertEqual<RottingCorpseKind?>(definition.Kind, configuration.Kind, $"{context} rots the kind whose upload list it names");
        AssertEqual(definition.RowCount, configuration.RowCount, $"{context} corpse height");
        AssertEqual(CorpseRowLayouts.TableAddress(RottingCorpseSpecies.Sidehopper), configuration.RowLayoutTable,
            $"{context} names the sidehopper tile-row layout production copies with");
        AssertEqual(configuration.Wrap, CorpseRowGeometry.Sidehopper(graphicsVariant).Wrap,
            $"{context} wrap is the extraction's row movers'");
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
        CorpseRotScheduleChecks.AssertFreshSchedule(expected.RowCount, state.Rot, $"dead sidehopper parameter {parameter1}");
        AssertEqual(expected.Kind, state.Flesh,
            $"dead sidehopper parameter {parameter1} rots the configured kind of corpse");
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
