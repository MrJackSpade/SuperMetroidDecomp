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

/// <summary>Checks Dead Torizo corpse metadata and its production initializer.</summary>
internal static class DeadTorizoCorpseDefinitionsVerification
{
    /// <summary>The bank-$A9 address of the dead Torizo's configuration record.</summary>
    private const ushort ConfigurationPointer = 0xdd58;

    /// <summary>Compares the decoded native configuration with the definition, the extraction's row movers and the slot state.</summary>
    internal static void VerifyDeadTorizoCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        CorpseRottingConfiguration configuration = CorpseRottingConfigurationCodec.Decode(rom, ConfigurationPointer);
        AssertEqual(DeadTorizoCorpseDefinitions.RowCount, configuration.RowCount, "dead Torizo corpse height");
        AssertEqual(CorpseRowLayouts.TableAddress(RottingCorpseSpecies.Torizo), configuration.RowLayoutTable,
            "dead Torizo configuration names the Torizo tile-row layout production copies with");
        AssertEqual(configuration.Wrap, CorpseRowGeometry.Torizo.Wrap, "dead Torizo wrap is the extraction's row movers'");
        AssertEqual<RottingCorpseKind?>(null, configuration.Kind,
            "dead Torizo has no generic transfer list; it uploads through its alternating halves");

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        Action<RoomEnemySlot> initialize = enemies.DeadTorizoBehavior.InitializeDeadTorizo;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        initialize(slot);

        DeadTorizoEnemyState state = (enemies.DeadTorizo ?? throw new InvalidOperationException("Verification requires enemies.DeadTorizo."));
        CorpseRotScheduleChecks.AssertFreshSchedule(DeadTorizoCorpseDefinitions.RowCount, state.Rot, "dead Torizo production");

        Console.WriteLine(
            "Dead Torizo corpse definitions: the decoded configuration's height, row layout and wrap, and the real initializer's own fresh rot schedule pass.");
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
