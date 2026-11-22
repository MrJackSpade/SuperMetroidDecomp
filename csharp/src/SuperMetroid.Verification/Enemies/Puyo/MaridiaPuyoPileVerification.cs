using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the six authored Puyo positions and staggered first-hop timers in the Maridia pile.</summary>
internal static class MaridiaPuyoPileVerification
{
    /// <summary>Loads retail room $D27E and confirms each Puyo stays grounded until its full-word cooldown underflows.</summary>
    internal static void VerifyMaridiaPuyoPile()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xd27e);
        var assets = LoadFixtureRoomAssets(room);
        var random = new SharedGameState(new SharedRandom());
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(FixtureEnemyTileArtwork()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { SeedRandom = random.SetRandomNumber });
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(room.State.EnemyPopulation, BgArtworkFixtures.For(new SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus()));
        // These are the six literal retail population records, not a constructed pile.
        Population[] population =
            [new Population(87,156,0x0f00), new Population(98,156,0x0f00), new Population(105,156,0x0f00),
             new Population(91,152,0x0e00), new Population(101,152,0x0e00), new Population(97,148,0x0d00)];
        AssertEqual(population.Length, enemies.Places.EnemyCount, "reported room has six authored Puyos");
        for (int i = 0; i < population.Length; i++)
        {
            var actor = enemies.Places.InUpdateOrder[i];
            AssertEqual(EnemyDefinitionId.Puyo, actor.Occupant?.DefinitionId, "pile actor is retail Puyo");
            AssertEqual(population[i].X, actor.Position.X.Whole, "native pile spawn X");
            AssertEqual(population[i].Y, actor.Position.Y.Whole, "native pile spawn Y");
        }
        var firstHop = Enumerable.Repeat(-1, population.Length).ToArray();
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(220, 128)), Health = 999, MaxHealth = 999 };
        for (int frame = 1; frame <= 3842; frame++)
        {
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(assets.LevelData) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
            for (int i = 0; i < population.Length; i++)
            {
                // A pile member's hop is seen as it leaves the spot its population placed it on, on its first airborne
                // step; its full-word initial cooldown holds it there until then.
                RoomEnemyPlace member = enemies.Places.InUpdateOrder[i];
                if (firstHop[i] < 0 && (member.Position.X.Whole != population[i].X || member.Position.Y.Whole != population[i].Y)) firstHop[i] = frame;
                if (frame <= population[i].Delay)
                {
                    AssertEqual(population[i].X, enemies.Places.InUpdateOrder[i].Position.X.Whole, "waiting pile X does not drift");
                    AssertEqual(population[i].Y, enemies.Places.InUpdateOrder[i].Position.Y.Whole, "waiting pile Y does not collapse");
                }
            }
        }
        for (int i = 0; i < population.Length; i++)
            // The update its cooldown underflows on starts the hop (native DEC/BPL), and its first airborne step moves it on
            // the next.
            AssertEqual(population[i].Delay + 2, firstHop[i], "native DEC/BPL starts hop on signed timer underflow, and the hop moves the member on the next update");
        Console.WriteLine($"Maridia $04/$10 authored Puyo pile: first hops at {string.Join(',', firstHop)} AI updates; all six spawn coordinates and waiting trajectories match retail data.");
    }

    /// <summary>Runs the focused retail Maridia Puyo pile timing check.</summary>
    [VerificationCommand("--maridia-puyo-pile")]
    internal static int MaridiaPuyoPileCommand()
    {
        Suite(nameof(VerifyMaridiaPuyoPile), () => VerifyMaridiaPuyoPile());
        return 0;
    }
}
