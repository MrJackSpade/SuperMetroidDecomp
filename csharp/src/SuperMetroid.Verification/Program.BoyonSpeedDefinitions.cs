using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledBoyonSpeeds(SuperMetroidAddressSpace rom)
    {
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
        Console.WriteLine("Boyon compiled curve: all 23 native bytes, 65536 index saturation cases and 16777216 real multiplication inputs match without a bus.");
    }
}
