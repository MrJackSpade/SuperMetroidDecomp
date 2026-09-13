using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeJumpDecisions(string rom, string trace)
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
            if (cells.Length != 14) throw new InvalidDataException("Malformed native Golden jump row.");
            ushort opcode = ushort.Parse(cells[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            ushort[] v = cells.Skip(1).Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            loaded.Head.XPosition = 256;
            loaded.Samus.XPosition = v[0];
            loaded.Head.Parameter1 = v[1];
            loaded.State.SamusSpaceJumpFrames = v[2];
            loaded.State.DecisionCounter = v[3];
            random.SetRandomNumber(v[5]);
            // Sentinels distinguish a rejected decision from a jump initialized with
            // the wrong speed/gravity or a timer accidentally overwritten every call.
            loaded.State.HorizontalVelocity = 17;
            loaded.State.VerticalVelocity = 19;
            loaded.State.VerticalAcceleration = 23;
            loaded.Head.InstructionTimer = 29;
            object?[] args = [loaded.Head, loaded.Samus, assets.LevelData, opcode,
                (ushort)0xcffe, v[4], (byte)0, false];
            bool handled = (bool)instruction.Invoke(loaded.Enemies, args)!;
            ushort[] actual = [(ushort)args[4]!, random.RandomNumber, loaded.State.DecisionCounter,
                loaded.State.HorizontalVelocity, loaded.State.VerticalVelocity,
                loaded.State.VerticalAcceleration, loaded.Head.InstructionTimer];
            if (!handled || (bool)args[7]! || !actual.SequenceEqual(v.Skip(6)))
                throw new InvalidDataException($"Golden jump differs: {line}; actual={string.Join(',', actual)}.");
            count++;
        }
        if (count != 768) throw new InvalidDataException($"Expected 768 native jump cases, got {count}.");
        Console.WriteLine($"Golden Torizo: {count} original-CPU jump decisions, RNG and velocity/gravity/timer writes match.");
        return 0;
    }
}
