using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeAmmoDecisions(string rom, string trace)
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
            ushort[] v = line.Split(',').Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            if (v.Length != 6) throw new InvalidDataException("Malformed Golden ammo decision row.");
            loaded.Samus.Missiles = v[0];
            loaded.Samus.XPosition = v[1];
            loaded.State.ReturnInstruction = 0x1234;
            random.SetRandomNumber(0x5678);
            // Native Y points to the first operand. The managed instruction cursor
            // includes its opcode; both read the same two branch targets in the ROM.
            object?[] args = [loaded.Head, loaded.Samus, assets.LevelData,
                TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack,
                (ushort)0xcffe, (ushort)0, checked((byte)v[2]), false];
            bool handled = (bool)instruction.Invoke(loaded.Enemies, args)!;
            if (!handled || (bool)args[7]! || (ushort)args[4]! != v[3] ||
                loaded.State.ReturnInstruction != v[4] || random.RandomNumber != v[5])
                throw new InvalidDataException($"Golden ammo decision differs: {line}; cursor={args[4]}, " +
                    $"link={loaded.State.ReturnInstruction}, random={random.RandomNumber}.");
            count++;
        }
        if (count != 1024) throw new InvalidDataException($"Expected 1024 ammo decisions, got {count}.");
        Console.WriteLine($"Golden Torizo: {count} original-CPU ammo/position/frame decisions match.");
        return 0;
    }
}
