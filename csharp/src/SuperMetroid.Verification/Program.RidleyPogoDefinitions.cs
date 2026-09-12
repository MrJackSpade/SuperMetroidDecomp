using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledRidleyPogo(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var reference = new (ushort X, ushort Y, ushort Up, ushort Down)[4, 6];
        for (int pattern = 0; pattern < 4; pattern++)
        for (int stage = 0; stage < 6; stage++)
        {
            int xTable = 0xa60000 | Word(EnemyRomTablePointers.Ridley.PogoHorizontalPathPointers + pattern * 2);
            int yTable = 0xa60000 | Word(EnemyRomTablePointers.Ridley.PogoVerticalPathPointers + pattern * 2);
            var native = (Word(xTable + stage * 2), Word(yTable + stage * 2),
                Word(EnemyRomTablePointers.Ridley.PogoUpwardAccelerationWords + stage * 2),
                Word(EnemyRomTablePointers.Ridley.PogoDownwardAccelerationWords + stage * 2));
            reference[pattern, stage] = native;
            AssertTrue(native == RidleyPogoDefinitions.Read(pattern, stage), "All indirect native pogo records and accelerations");
        }
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        int reads = 0, advances = 0;
        ushort seed = 0;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => { reads++; return seed; }));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => { advances++; return (ushort)(seed ^ 1); }));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeNorfairRidleyPogoVelocity", flags)!
            .CreateDelegate<Action<RidleyEnemyState>>(enemies);
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
            state.HorizontalVelocity = previous;
            state.FunctionTimer = 0x1234;
            initialize(state);
            var expected = reference[raw & 3, Math.Min(health, (ushort)3) + 2];
            AssertEqual(0, advances, "Native Ridley pogo initializer must not advance RNG");
            AssertEqual(1, reads, "Native Ridley pogo initializer reads current RNG once");
            AssertEqual(unchecked((ushort)((short)previous < 0 ? -expected.X : expected.X)), state.HorizontalVelocity, "Pogo selected horizontal magnitude/sign");
            AssertEqual(expected.Y, state.VerticalVelocity, "Pogo selected vertical speed");
            AssertEqual(expected.Up, state.PogoUpwardAcceleration, "Pogo upward acceleration");
            AssertEqual(expected.Down, state.PogoDownwardAcceleration, "Pogo downward acceleration");
            AssertEqual(health, state.HealthStage, "Pogo preserves health-stage state");
            AssertEqual((ushort)0x1234, state.FunctionTimer, "Pogo preserves caller timer");
            cases++;
        }
        AssertEqual(1572864, cases, "All random words, health stages and horizontal signs");
        Console.WriteLine("Ridley pogo: all indirect native records and 1572864 actual initializations preserve RNG, signed speed and asymmetric acceleration without ROM reads.");
    }
}
