using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledBoyonSpeeds(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBoyon", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot actor = enemies.Slots[0];
        for (int height = 0; height < 9; height++)
        {
            for (int multiplier = 0; multiplier < 8; multiplier++)
            {
                actor.Parameter1 = (ushort)((height << 8) | multiplier);
                initialize(actor);
                BoyonEnemyState initialized = enemies.BoyonStates[0]!;
                AssertEqual(Word(0xa286df + multiplier * 2), initialized.SpeedMultiplier, "Boyon real initialized multiplier without bus");
                AssertEqual(Word(0xa286ef + height * 2), initialized.JumpHeight, "Boyon real initialized height without bus");
            }
        }
        actor.Parameter1 = 8;
        AssertThrows<InvalidDataException>(() => initialize(actor), "Boyon rejected multiplier unchanged");
        actor.Parameter1 = 0x900;
        AssertThrows<InvalidDataException>(() => initialize(actor), "Boyon rejected height unchanged");
        var multiply = typeof(RoomEnemySystem).GetMethod("MultiplyBoyonCurveEntry", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<BoyonEnemyState, ushort>>();
        var state = new BoyonEnemyState(new RoomEnemySystem().Slots[0]);
        for (int index = 0; index <= ushort.MaxValue; index++)
        {
            byte sample = index < 23 ? rom.ReadByte(0xa28701 + index) : (byte)255;
            AssertEqual(sample, BoyonSpeedDefinitions.Sample((ushort)index), "Boyon native curve/saturation");
            state.SpeedTableIndex = (ushort)index;
            for (int multiplier = 0; multiplier < 256; multiplier++)
            {
                // Exercise every effective hardware input, with nonzero discarded high bytes.
                state.SpeedMultiplier = (ushort)(0xa500 | multiplier);
                AssertEqual((ushort)(sample * multiplier), multiply(state), "Boyon real byte-width multiply without bus");
            }
        }
        Console.WriteLine("Boyon compiled definitions: 72 real initializers, 23 native curve bytes, 65536 index saturation cases and 16777216 real multiplication inputs match without a bus.");
    }
}
