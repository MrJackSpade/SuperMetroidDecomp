using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SharedSlopeHeightsVerification;
using static SuiteTimingVerification;

internal static class RidleyPogoDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley pogo")]
    internal static void VerifyCompiledRidleyPogo(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyRidleyAttackTimerRandomReads), () => VerifyRidleyAttackTimerRandomReads(rom));
        var reference = new (ushort X, ushort Y, ushort Up, ushort Down)[4, 6];
        for (int pattern = 0; pattern < 4; pattern++)
        for (int stage = 0; stage < 6; stage++)
        {
            int xTable = 0xa60000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoHorizontalPathPointers + pattern * 2));
            int yTable = 0xa60000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoVerticalPathPointers + pattern * 2));
            var native = (CartridgeImportSource.Require(rom).ReadCartridgeWord((xTable + stage * 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((yTable + stage * 2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoUpwardAccelerationWords + stage * 2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ridley.PogoDownwardAccelerationWords + stage * 2)));
            reference[pattern, stage] = native;
            AssertTrue(native == RidleyPogoDefinitions.Read(pattern, stage), "All indirect native pogo records and accelerations");
        }
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        int reads = 0, advances = 0;
        ushort seed = 0;
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => { reads++; return seed; });
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { advances++; return (ushort)(seed ^ 1); });
        Action<RidleyEnemyState> initialize = enemies.RidleyBehavior.InitializeNorfairRidleyPogoVelocity;
        var state = new RidleyEnemyState();
        ushort[] healthStages = [0, 1, 2, 3, 4, 0xffff];
        ushort[] velocities = [0, 0x7fff, 0x8000, 0xffff];
        int cases = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        foreach (ushort health in healthStages)
        foreach (ushort previous in velocities)
        {
            seed = (ushort)raw;
            reads = advances = 0;
            state.HealthStage = health;
            state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(previous));
            state.FunctionTimer = NativeCountdown.Start(0x1234);
            initialize(state);
            var expected = reference[raw & 3, Math.Min(health, (ushort)3) + 2];
            AssertEqual(0, advances, "Native Ridley pogo initializer must not advance RNG");
            AssertEqual(1, reads, "Native Ridley pogo initializer reads current RNG once");
            AssertEqual(unchecked((ushort)((short)previous < 0 ? -expected.X : expected.X)), state.Velocity.X.RawValue, "Pogo selected horizontal magnitude/sign");
            AssertEqual(expected.Y, state.Velocity.Y.RawValue, "Pogo selected vertical speed");
            AssertEqual(expected.Up, state.PogoUpwardAcceleration, "Pogo upward acceleration");
            AssertEqual(expected.Down, state.PogoDownwardAcceleration, "Pogo downward acceleration");
            AssertEqual(health, state.HealthStage, "Pogo preserves health-stage state");
            AssertEqual((ushort)0x1234, state.FunctionTimer.RawValue, "Pogo preserves caller timer");
            cases++;
        }
        AssertEqual(1572864, cases, "All random words, health stages and horizontal signs");
        Console.WriteLine("Ridley pogo: all indirect native records and 1572864 actual initializations preserve RNG, signed speed and asymmetric acceleration without ROM reads.");
    }

    private static void VerifyRidleyAttackTimerRandomReads(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        int reads = 0, advances = 0;
        ushort seed = 0;
        enemies.BindFixtureBus(rom);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => { reads++; return seed; });
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { advances++; return (ushort)(seed ^ 1); });
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?, ushort, RoomLevelData?> run = enemies.RidleyBehavior.RunNorfairRidleyFunction;
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> attack = enemies.RidleyBehavior.TickNorfairRidleyGroundAttackMoveToHeight;
        var slot = enemies.SlotTable.Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            seed = (ushort)raw;
            reads = advances = 0;
            var state = new RidleyEnemyState { Function = RidleyAiFunction.NorfairPogoSetup };
            slot.Position = slot.Position.WithWhole(new(128, 288));
            // Null Samus deliberately takes the non-spin handoff after the timer write;
            // it avoids consuming a timer tick while exercising the real setup dispatcher.
            run(slot, state, null, 0, null);
            AssertEqual(0, advances, "Native Ridley pogo setup must not advance RNG");
            AssertEqual(1, reads, "Pogo setup samples current RNG once");
            AssertEqual((ushort)((raw & 31) + 32), state.FunctionTimer.RawValue, "Pogo setup exact native timer");
            AssertEqual(RidleyAiFunction.NorfairFireballMoveToSide, state.Function, "Pogo non-spin handoff retained");

            reads = advances = 0;
            state.Function = RidleyAiFunction.NorfairFireballMoveToHeight;
            state.FunctionTimer = NativeCountdown.Start(0);
            state.TailSegments = Enumerable.Range(0, 7).Select(_ => new RidleyTailSegment()).ToArray();
            attack(slot, state, null);
            AssertEqual(0, advances, "Native Ridley fireball setup must not advance RNG");
            AssertEqual(2, reads, "Pogo velocity and fireball timer sample the same current word");
            AssertEqual((ushort)((raw & 63) + 128), state.FunctionTimer.RawValue, "Fireball attack exact native timer");
            AssertEqual(RidleyAiFunction.NorfairFireballAttack, state.Function, "Fireball setup handoff retained");
        }
        Console.WriteLine("Ridley attack timers: both real setup paths preserve RNG and exact duration for all 65536 current words.");
    }
}
