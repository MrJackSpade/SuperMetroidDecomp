using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeHealthDecisions(string rom, string trace)
        => CompareNativeDecisions(rom, trace, distance: false);

    public static int CompareNativeDistanceDecisions(string rom, string trace)
        => CompareNativeDecisions(rom, trace, distance: true);

    private static int CompareNativeDecisions(string rom, string trace, bool distance)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var loaded = Load(bus, room, assets, false, () => { });
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        var next = (Func<ushort>)typeof(RoomEnemySystem).GetField("_nextRandom", hidden)!.GetValue(loaded.Enemies)!;
        var random = (Bank80SystemState)next.Target!;
        var instruction = typeof(RoomEnemySystem).GetMethod("TryProcessBombTorizoInstruction", hidden)!;
        int count = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] cells = line.Split(',');
            if (cells.Length != (distance ? 9 : 8)) throw new InvalidDataException("Malformed native Golden Torizo decision row.");
            ushort opcode = ushort.Parse(cells[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            ushort[] values = cells.Skip(1).Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            int output = distance ? 4 : 3;
            if (distance)
            {
                loaded.Head.XPosition = 256;
                loaded.Samus.XPosition = values[0];
                loaded.Head.Parameter1 = values[1];
                loaded.Samus.Pose = checked((byte)values[2]);
            }
            else
            {
                loaded.Head.Health = values[0];
                loaded.Head.Parameter2 = values[1];
            }
            random.SetRandomNumber(values[output - 1]);
            loaded.State.ReturnInstruction = 0x1234;
            loaded.State.DecisionCounter = 9;
            // Native Y points at the operand; the translated dispatcher cursor includes
            // its preceding opcode. Both read exactly the same cartridge operand word.
            object?[] args = [loaded.Head, loaded.Samus, assets.LevelData, opcode,
                (ushort)0xcffe, (ushort)0, (byte)0, false];
            bool handled = (bool)instruction.Invoke(loaded.Enemies, args)!;
            if (!handled || (bool)args[7]! || (ushort)args[4]! != values[output] ||
                random.RandomNumber != values[output + 1] || loaded.State.ReturnInstruction != values[output + 2] ||
                loaded.State.DecisionCounter != values[output + 3])
                throw new InvalidDataException($"Golden decision differs: {line}; got cursor={args[4]}, " +
                    $"random={random.RandomNumber}, link={loaded.State.ReturnInstruction}, count={loaded.State.DecisionCounter}.");
            count++;
        }
        int expectedCount = distance ? 1024 : 240;
        if (count != expectedCount) throw new InvalidDataException($"Expected {expectedCount} native decision cases, got {count}.");
        Console.WriteLine($"Golden Torizo: {count} original-CPU {(distance ? "distance/facing/pose" : "health/stun")}/RNG/link/counter decisions match.");
        return 0;
    }
}
