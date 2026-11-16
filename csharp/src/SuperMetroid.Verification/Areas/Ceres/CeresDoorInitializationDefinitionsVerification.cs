using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Areas.Ceres;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the seven Ceres doors and walls a placement places against the cartridge's selector tables.</summary>
internal static class CeresDoorInitializationDefinitionsVerification
{
    /// <summary>
    /// Starts each door or wall through production initialization (#1953) and compares the main behaviour and animation
    /// its actor starts with the cartridge's function and instruction selector words, under a ROM read guard.
    /// </summary>
    internal static void VerifyCeresDoorInitializationDefinitions(CartridgeImportAddressSpace rom)
    {
        const int instructionTable = 0xa6f52c;
        const int functionTable = 0xa6f72b;

        var guarded = NewCeresDoorInitializationReadGuard(rom);

        for (ushort selector = 0; selector < 7; selector++)
        {
            (EnemyDefinitionId actor, EnemySpawnSettings settings) = SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.CeresDoorHeaderPlacement(selector);
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork);
            enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.StartCeresDoorOrWall(actor, settings);

            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((functionTable + selector * 2)),
                CeresNativeWords.MainBehaviourWord(place),
                $"production Ceres {actor} {settings} main behaviour, function selector {selector}");
            AnimationPlayback<CeresDoorSegment> playback = place.RequireSpeciesState<CeresDoorAnimationState>().Playback;
            CeresDoorSegment started = CeresDoorAnimations.StartingAt.Keys.Single(segment =>
                new AnimationPlayback<CeresDoorSegment>(CeresDoorAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation).Equals(playback));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + selector * 2)),
                NativeEnemyAnimationOracle.EntryOf(CeresDoorNativeListAddresses.Entries, started),
                $"production Ceres {actor} {settings} animation, instruction selector {selector}");
            AssertEqual((ushort)1, place.AnimationClock.Hold.RawValue,
                $"production Ceres {actor} {settings} instruction timer");
        }

        AssertThrows<InvalidDataException>(
            () => SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.CeresDoorHeaderPlacement(7),
            "Ceres door selector past the doors and walls");
        Console.WriteLine(
            "Ceres door initialization: all fourteen native selector words match the seven production doors and walls with both source tables forbidden.");
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
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
