using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
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
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { SeedRandom = random.SetRandomNumber }) { TileArtwork = FixtureEnemyTileArtwork() };
        enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, room.State.EnemyGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new SnesCgram()));
        // These are the six literal retail population records, not a constructed pile.
        Population[] population =
            [new Population(87,156,0x0f00), new Population(98,156,0x0f00), new Population(105,156,0x0f00),
             new Population(91,152,0x0e00), new Population(101,152,0x0e00), new Population(97,148,0x0d00)];
        AssertEqual(population.Length, enemies.SlotTable.EnemyCount, "reported room has six authored Puyos");
        for (int i = 0; i < population.Length; i++)
        {
            var actor = enemies.SlotTable.Slots[i];
            AssertEqual(EnemyDefinitionId.Puyo, actor.EnemyDefinitionPointer, "pile actor is retail Puyo");
            AssertEqual(population[i].X, actor.Position.X.Whole, "native pile spawn X");
            AssertEqual(population[i].Y, actor.Position.Y.Whole, "native pile spawn Y");
            AssertEqual(population[i].Delay, (enemies.PuyoStates[i] ?? throw new InvalidOperationException("Verification requires enemies.PuyoStates[i].")).HopCooldownTimer.RawValue, "native full-word initial cooldown");
        }
        var firstHop = Enumerable.Repeat(-1, population.Length).ToArray();
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(220, 128)), Health = 999, MaxHealth = 999 };
        for (int frame = 1; frame <= 3841; frame++)
        {
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData });
            for (int i = 0; i < population.Length; i++)
            {
                var state = enemies.PuyoStates[i];
                if (firstHop[i] < 0 && (state ?? throw new InvalidOperationException("Verification requires state.")).Function == PuyoEnemyFunction.Airborne) firstHop[i] = frame;
                if (frame <= population[i].Delay)
                {
                    AssertEqual(PuyoEnemyFunction.Grounded, (state ?? throw new InvalidOperationException("Verification requires state.")).Function, "native cooldown holds pile member grounded");
                    AssertEqual(population[i].X, enemies.SlotTable.Slots[i].Position.X.Whole, "waiting pile X does not drift");
                    AssertEqual(population[i].Y, enemies.SlotTable.Slots[i].Position.Y.Whole, "waiting pile Y does not collapse");
                }
            }
        }
        for (int i = 0; i < population.Length; i++)
            AssertEqual(population[i].Delay + 1, firstHop[i], "native DEC/BPL starts hop on signed timer underflow");
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
