using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeSuperAim(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var loaded = Load(bus, room, CartridgeRoomAssets.Load(bus, room), false, () => { });
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetMethod("SpawnGoldenTorizoSuperMissile", hidden)!
            .Invoke(loaded.Enemies, [loaded.Head]);
        var projectile = loaded.Enemies.EnemyProjectiles.Single(p => p.IsActive);
        projectile.XPosition = projectile.YPosition = 512;
        var aim = typeof(RoomEnemySystem).GetMethod("SetGoldenTorizoSuperMissileVelocity",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        int count = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] cells = line.Split(',');
            if (cells.Length != 5 || cells[0] is not ("B269" or "B272"))
                throw new InvalidDataException("Malformed Golden Super aim row.");
            ushort[] v = cells.Skip(1).Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            loaded.Samus.XPosition = v[0]; loaded.Samus.YPosition = v[1];
            aim.Invoke(null, [projectile, loaded.Samus, cells[0] == "B272"]);
            if (projectile.XVelocity != v[2] || projectile.YVelocity != v[3])
                throw new InvalidDataException($"Golden Super aim differs: {line}; actual={projectile.XVelocity},{projectile.YVelocity}.");
            count++;
        }
        if (count != 50) throw new InvalidDataException($"Expected 50 Super aim cases, got {count}.");
        Console.WriteLine("Golden Torizo: all 50 original-CPU Super aim velocities match.");
        return 0;
    }

    public static int CompareNativeProjectileInitializers(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var loaded = Load(bus, room, CartridgeRoomAssets.Load(bus, room), false, () => { });
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        var next = (Func<ushort>)typeof(RoomEnemySystem).GetField("_nextRandom", hidden)!.GetValue(loaded.Enemies)!;
        var random = (Bank80SystemState)next.Target!;
        int count = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] cells = line.Split(',');
            if (cells.Length != 9) throw new InvalidDataException("Malformed native Golden projectile row.");
            ushort[] v = cells.Skip(1).Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            // Independent native entry-point -> production operation mapping. Clear the
            // pool between samples so allocation exhaustion cannot masquerade as a match.
            string method = cells[0] switch
            {
                "AC7C" => "SpawnGoldenTorizoChozoOrb",
                "AE15" => "SpawnGoldenTorizoSonicBoom",
                "B001" => "SpawnGoldenTorizoEgg",
                "B1CE" => "SpawnGoldenTorizoSuperMissile",
                "B328" => "SpawnGoldenTorizoEyeBeam",
                _ => throw new InvalidDataException($"Unexpected native initializer {cells[0]}.")
            };
            foreach (var slot in loaded.Enemies.EnemyProjectiles) slot.Clear();
            loaded.Head.XPosition = 256;
            loaded.Head.YPosition = 384;
            loaded.Head.Parameter1 = v[0];
            random.SetRandomNumber(v[1]);
            object?[] args = cells[0] is "AE15" or "B328"
                ? [loaded.Head, (ushort)0] : [loaded.Head];
            typeof(RoomEnemySystem).GetMethod(method, hidden)!.Invoke(loaded.Enemies, args);
            var projectile = loaded.Enemies.EnemyProjectiles.Single(p => p.IsActive);
            ushort[] actual = [projectile.XPosition, projectile.YPosition, projectile.XVelocity,
                projectile.YVelocity, projectile.InstructionPointer, random.RandomNumber];
            if (!actual.SequenceEqual(v.Skip(2)))
                throw new InvalidDataException($"Golden projectile differs: {line}; actual={string.Join(',', actual)}.");
            count++;
        }
        if (count != 2560) throw new InvalidDataException($"Expected 2560 native projectile cases, got {count}.");
        Console.WriteLine($"Golden Torizo: {count} original-CPU projectile positions, velocities, lists and RNG results match.");
        return 0;
    }
}
