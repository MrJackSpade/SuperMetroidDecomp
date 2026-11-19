using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Ridley's pogo records, speed selection, and RNG sampling in actual enemy setup.</summary>
internal static class RidleyPogoDefinitionsVerification
{
    /// <summary>
    /// Compares each health band's launches with the cartridge's pogo records, then every seed, band and prior speed
    /// through the real initializer. The cartridge reads its pogo tables two stages past the health stage; its first two
    /// stages are never read, so no band holds them (#1934).
    /// </summary>
    internal static void VerifyCompiledRidleyPogo(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyRidleyAttackTimerRandomReads), () => VerifyRidleyAttackTimerRandomReads());
        NorfairRidleyHealthBand[] bands = RidleyNativeWords.HealthBandsInStageOrder;
        var reference = new RidleyPogoDefinitionsReference[4, 4];
        for (int pattern = 0; pattern < 4; pattern++)
        for (int band = 0; band < 4; band++)
        {
            int stage = band + 2;
            int xTable = 0xa60000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoHorizontalPathPointers + pattern * 2));
            int yTable = 0xa60000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoVerticalPathPointers + pattern * 2));
            var native = new RidleyPogoDefinitionsReference(CartridgeImportSource.Require(rom).ReadCartridgeWord((xTable + stage * 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((yTable + stage * 2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoUpwardAccelerationWords + stage * 2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoDownwardAccelerationWords + stage * 2)));
            reference[pattern, band] = native;
            RidleyPogoDefinition compiled = bands[band].PogoLaunch.Choose(pattern);
            AssertTrue(native == new RidleyPogoDefinitionsReference(compiled.Velocity.X.RawValue, compiled.Velocity.Y.RawValue, compiled.UpwardAcceleration.RawValue, compiled.DownwardAcceleration.RawValue),
                "All indirect native pogo records and accelerations");
        }
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        int reads = 0, advances = 0;
        ushort seed = 0;
        enemies.VerificationServices().ReadRandom = () => { reads++; return seed; };
        enemies.VerificationServices().NextRandom = () => { advances++; return (ushort)(seed ^ 1); };
        Action<NorfairRidleyState> initialize = enemies.NorfairRidleyBehavior.Combat.InitializeNorfairRidleyPogoVelocity;
        var state = new NorfairRidleyState();
        HorizontalDirection[] sides = [HorizontalDirection.Left, HorizontalDirection.Right];
        int cases = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        for (int band = 0; band < bands.Length; band++)
        foreach (HorizontalDirection side in sides)
        {
            seed = (ushort)raw;
            reads = advances = 0;
            state.HealthBand = bands[band];
            state.PogoBounce = side;
            state.PhaseTimer.LastFor(0x1235);
            initialize(state);
            var expected = reference[raw & 3, band];
            AssertEqual(0, advances, "Native Ridley pogo initializer must not advance RNG");
            AssertEqual(1, reads, "Native Ridley pogo initializer reads current RNG once");
            AssertEqual(unchecked((ushort)(side == HorizontalDirection.Left ? -expected.X : expected.X)), state.Flight.Velocity.X.RawValue, "Pogo selected sideways speed toward the bounce's side");
            AssertEqual(expected.Y, state.Flight.Velocity.Y.RawValue, "Pogo selected vertical speed");
            AssertEqual(expected.Up, state.PogoUpwardAcceleration.RawValue, "Pogo upward acceleration");
            AssertEqual(expected.Down, state.PogoDownwardAcceleration.RawValue, "Pogo downward acceleration");
            AssertTrue(ReferenceEquals(bands[band], state.HealthBand), "Pogo preserves the health band");
            AssertEqual(0x1234, state.PhaseTimer.CartridgeCount, "Pogo preserves caller timer");
            cases++;
        }
        AssertEqual(524288, cases, "All random words, health bands and bounce sides");
        Console.WriteLine("Ridley pogo: all indirect native records and 1572864 actual initializations preserve RNG, signed speed and asymmetric acceleration without ROM reads.");
    }

    /// <summary>Checks the wall sweep and pogo timers consume the current RNG word without advancing it.</summary>
    private static void VerifyRidleyAttackTimerRandomReads()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        int reads = 0, advances = 0;
        ushort seed = 0;
        enemies.VerificationServices().ReadRandom = () => { reads++; return seed; };
        enemies.VerificationServices().NextRandom = () => { advances++; return (ushort)(seed ^ 1); };
        // The power bomb explosion of idle bombs, none exploding, which a missed lunge would dodge.
        var powerBomb = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandalonePowerBombExplosion();
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState, GameplayAction, RoomLevelData> run = (ridley, ridleyState, target, input, level) =>
            enemies.NorfairRidleyBehavior.RunNorfairRidleyPhase(ridley, ridleyState, target, input, level, powerBomb);
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState> attack = enemies.NorfairRidleyBehavior.Combat.TickNorfairRidleyPogoMoveToHeight;
        var place = enemies.Places.InUpdateOrder[0];
        // One new Samus and one open room serve every word: neither setup changes them.
        SamusState samus = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus();
        RoomLevelData openRoom = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            seed = (ushort)raw;
            reads = advances = 0;
            var state = new NorfairRidleyState { Phase = NorfairRidleyPhase.WallSweepSetup };
            place.Position = place.Position.WithWhole(new(128, 288));
            // Null Samus deliberately takes the non-spin handoff after the timer write;
            // it avoids consuming a timer tick while exercising the real setup dispatcher.
            run(place, state, samus, 0, openRoom);
            AssertEqual(0, advances, "Native Ridley wall sweep setup must not advance RNG");
            AssertEqual(1, reads, "Wall sweep setup samples current RNG once");
            AssertEqual((ushort)((raw & 31) + 32), state.PhaseTimer.CartridgeCount, "Wall sweep setup exact native timer");
            AssertEqual(NorfairRidleyPhase.PogoMoveToSide, state.Phase, "Wall sweep non-spin handoff to the pogo retained");

            reads = advances = 0;
            state.Phase = NorfairRidleyPhase.PogoMoveToHeight;
            state.PhaseTimer.LastFor(1);
            RidleyTailFixtures.BlankLinks(state.Tail);
            attack(place, state, samus);
            AssertEqual(0, advances, "Native Ridley pogo start must not advance RNG");
            AssertEqual(2, reads, "Pogo velocity and pogo timer sample the same current word");
            AssertEqual((ushort)((raw & 63) + 128), state.PhaseTimer.CartridgeCount, "Pogo attack exact native timer");
            AssertEqual(NorfairRidleyPhase.PogoFall, state.Phase, "Pogo start handoff retained");
        }
        Console.WriteLine("Ridley attack timers: both real setup paths preserve RNG and exact duration for all 65536 current words.");
    }
}
