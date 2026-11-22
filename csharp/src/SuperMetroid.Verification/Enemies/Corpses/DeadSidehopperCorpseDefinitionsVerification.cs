using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks alive and dead Sidehopper corpse metadata and production initialization.</summary>
internal static class DeadSidehopperCorpseDefinitionsVerification
{
    /// <summary>Compares both native configuration records and initialized rotting state without metadata rereads.</summary>
    internal static void VerifyDeadSidehopperCorpseDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            configurationPointer: 0xdd68,
            graphicsVariant: 0));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            configurationPointer: 0xdd78,
            graphicsVariant: 2));

        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            parameter1: 0,
            DeadSidehopperAction.AliveWaitForCamera));
        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            parameter1: 2,
            DeadSidehopperAction.WaitForSamusCollision));

        Console.WriteLine(
            "Dead sidehopper corpse definitions: both decoded configuration records' upload lists and heights, and both production initializers' own fresh rot schedules pass.");
    }

    /// <summary>
    /// Compares one corpse record, decoded by the codec, with the typed definition: the upload list it names, which the
    /// extraction finds the corpse's characters by, and the corpse's height.
    /// </summary>
    private static void VerifyDefinition(
        CartridgeImportAddressSpace rom,
        ushort configurationPointer,
        int graphicsVariant)
    {
        CorpseRottingConfiguration configuration = CorpseRottingConfigurationCodec.Decode(rom, configurationPointer);
        string context = $"dead sidehopper configuration $A9:{configurationPointer:X4}";
        AssertEqual<CorpseRowUploadList?>(graphicsVariant == 0 ? CorpseRowUploadList.SidehopperBabyMetroidVictim : CorpseRowUploadList.SidehopperAlreadyDead, configuration.UploadList,
            $"{context} names the row upload list the extraction finds the corpse's characters by");
        AssertEqual(DeadSidehopperCorpseDefinitions.RowCount, configuration.RowCount, $"{context} corpse height");
    }

    /// <summary>
    /// Checks the state installed for one alive or initially dead enemy parameter: the parameter chooses the living victim,
    /// which waits for the camera, or the corpse already dead, which waits for Samus to touch it.
    /// </summary>
    private static void VerifyProductionInitialization(
        ushort parameter1,
        DeadSidehopperAction startingAction)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork);
        // The corpse its population entry's first parameter lays out, set up as its room's population sets it up (#2554).
        _ = enemies.LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.CorpseSidehopper,
            EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.CorpseSidehopper, PopulationWords.Parameters(parameter1, 0))));

        DeadSidehopperEnemyState state = (enemies.DeadSidehoppers[0] ?? throw new InvalidOperationException("Verification requires enemies.DeadSidehoppers[0]."));
        AssertEqual<DeadSidehopperAction?>(startingAction, state.Action,
            $"dead sidehopper parameter {parameter1} starting function");
        CorpseRotScheduleChecks.AssertFreshSchedule(DeadSidehopperCorpseDefinitions.RowCount, state.Rot, $"dead sidehopper parameter {parameter1}");
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
