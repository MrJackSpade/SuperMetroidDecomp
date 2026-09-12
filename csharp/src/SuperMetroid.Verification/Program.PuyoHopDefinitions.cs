using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPuyoHops(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var calculate = typeof(RoomEnemySystem).GetMethod("CalculateInitialPuyoHopSpeed", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<PuyoEnemyState>>();
        var state = new PuyoEnemyState(new RoomEnemySystem().Slots[0]);
        for (ushort index = 0; index < 56; index += 8)
        {
            var hop = PuyoHopDefinitions.FromByteIndex(index);
            AssertEqual(Word(0xa29a07 + index), hop.Height, "Puyo native hop height");
            AssertEqual(Word(0xa29a09 + index), hop.XSpeed, "Puyo native horizontal speed");
            AssertEqual(Word(0xa29a0b + index), hop.YIndexDelta, "Puyo native gravity/drop delta");
            AssertEqual(Word(0xa29a0d + index), (ushort)hop.Function, "Puyo native airborne callback");
            if (hop.Function == PuyoAirborneFunction.Dropping)
                continue; // Dropping bypasses hop initialization and uses constant speed.
            ushort time = 0, distance = 0;
            ushort height = Word(0xa29a07 + index);
            ushort target = unchecked((ushort)((height << 8) | (height >> 8)));
            do
            {
                time = unchecked((ushort)(time + Word(0xa29a09 + index)));
                distance = unchecked((ushort)(distance + Word(0xa08390 + (time >> 8) * 8)));
            } while (unchecked((short)(target - distance)) >= 0);
            state.HopTableIndex = index;
            calculate(state);
            AssertEqual(time, state.YSpeedTableIndex, "Puyo real initial curve index without bus");
            AssertEqual(time / 2, state.InitialYSpeedTableIndexHalf, "Puyo half threshold");
            AssertEqual(time / 2 + time / 4, state.InitialYSpeedTableIndexThreeQuarters, "Puyo three-quarter threshold");
        }
        foreach (ushort invalid in new ushort[] { 1, 7, 49, 56, 65535 })
            AssertThrows<InvalidDataException>(() => PuyoHopDefinitions.FromByteIndex(invalid), "Puyo invalid byte selector");
        Console.WriteLine("Puyo definitions: all seven native records and six real initial hop integrations match without a bus; dropping remains a separate constant-speed path.");
    }
}
