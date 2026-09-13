using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeHealthDecisions(string rom, string trace)
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
            if (cells.Length != 8) throw new InvalidDataException("Malformed native Golden Torizo decision row.");
            ushort opcode = ushort.Parse(cells[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            ushort[] values = cells.Skip(1).Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            loaded.Head.Health = values[0];
            loaded.Head.Parameter2 = values[1];
            random.SetRandomNumber(values[2]);
            loaded.State.ReturnInstruction = 0x1234;
            loaded.State.DecisionCounter = 9;
            // Native Y points at the operand; the translated dispatcher cursor includes
            // its preceding opcode. Both read exactly the same cartridge operand word.
            object?[] args = [loaded.Head, loaded.Samus, assets.LevelData, opcode,
                (ushort)0xcffe, (ushort)0, (byte)0, false];
            bool handled = (bool)instruction.Invoke(loaded.Enemies, args)!;
            if (!handled || (bool)args[7]! || (ushort)args[4]! != values[3] ||
                random.RandomNumber != values[4] || loaded.State.ReturnInstruction != values[5] ||
                loaded.State.DecisionCounter != values[6])
                throw new InvalidDataException($"Golden decision differs: {line}; got cursor={args[4]}, " +
                    $"random={random.RandomNumber}, link={loaded.State.ReturnInstruction}, count={loaded.State.DecisionCounter}.");
            count++;
        }
        if (count != 240) throw new InvalidDataException($"Expected 240 native decision cases, got {count}.");
        Console.WriteLine($"Golden Torizo: {count} original-CPU health/stun/RNG/link/counter decisions match.");
        return 0;
    }
}
