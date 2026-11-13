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

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
        Action<RoomEnemySlot> initialize = enemies.DeadTorizoBehavior.InitializeDeadTorizo;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        initialize(slot);

        DeadTorizoEnemyState state = (enemies.DeadTorizo ?? throw new InvalidOperationException("Verification requires enemies.DeadTorizo."));
        CorpseRotScheduleChecks.AssertFreshSchedule(DeadTorizoCorpseDefinitions.RowCount, state.Rot, "dead Torizo production");

        Console.WriteLine(
            "Dead Torizo corpse definitions: the decoded configuration's height and upload list, and the real initializer's own fresh rot schedule pass.");
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
