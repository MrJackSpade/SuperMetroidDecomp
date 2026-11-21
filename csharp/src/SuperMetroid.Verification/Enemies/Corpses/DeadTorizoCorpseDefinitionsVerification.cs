using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Dead Torizo corpse metadata and its production initializer.</summary>
internal static class DeadTorizoCorpseDefinitionsVerification
{
    /// <summary>The bank-$A9 address of the dead Torizo's configuration record.</summary>
    private const ushort ConfigurationPointer = 0xdd58;

    /// <summary>Compares the decoded native configuration with the definition and the slot state.</summary>
    internal static void VerifyDeadTorizoCorpseDefinitions(CartridgeImportAddressSpace rom)
    {
        CorpseRottingConfiguration configuration = CorpseRottingConfigurationCodec.Decode(rom, ConfigurationPointer);
        AssertEqual(DeadTorizoCorpseDefinitions.RowCount, configuration.RowCount, "dead Torizo corpse height");
        AssertEqual<CorpseRowUploadList?>(null, configuration.UploadList,
            "dead Torizo has no generic transfer list; it uploads through its alternating halves");

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork);
        static void initialize(RoomEnemyPlace corpse)
        {
            corpse.Bind(EnemyDefinitionId.DeadTorizo, corpse.KeptDefinition);
            corpse.SetUpAsPlaced();
        }
        // A corpse keeps its own state on its own slot, wherever the room lists it (#1808): one listed first and one
        // listed later each start their own fresh schedule.
        int[] places = [0, 3];
        foreach (int place in places)
            initialize(enemies.Places.InUpdateOrder[place]);

        foreach (int place in places)
        {
            DeadTorizoEnemyState state = enemies.DeadTorizos[place] ?? throw new InvalidOperationException($"The dead Torizo in slot {place} has no state.");
            CorpseRotScheduleChecks.AssertFreshSchedule(DeadTorizoCorpseDefinitions.RowCount, state.Rot, $"dead Torizo production in slot {place}");
        }
        AssertTrue(!ReferenceEquals(enemies.DeadTorizos[places[0]], enemies.DeadTorizos[places[1]]), "each dead Torizo has its own state");

        Console.WriteLine(
            "Dead Torizo corpse definitions: the decoded configuration's height and upload list, and the real initializer's own fresh rot schedule on each corpse's slot pass.");
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
