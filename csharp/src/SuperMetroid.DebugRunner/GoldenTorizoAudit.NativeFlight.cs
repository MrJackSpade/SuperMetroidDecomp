using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeFlight(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var loaded = Load(bus, room, assets, false, () => { });
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        var next = (Func<ushort>)typeof(RoomEnemySystem).GetField("_nextRandom", hidden)!.GetValue(loaded.Enemies)!;
        var random = (Bank80SystemState)next.Target!;
        RoomEnemyProjectileSlot? projectile = null;
        int rows = 0, samples = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            ushort[] v = line.Split(',').Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            if (v.Length != 12 || v[0] > 1) throw new InvalidDataException("Malformed Golden flight row.");
            if (v[3] == 0)
            {
                foreach (var slot in loaded.Enemies.EnemyProjectiles) slot.Clear();
                loaded.Head.XPosition = 256;
                loaded.Head.YPosition = 384;
                loaded.Head.Parameter1 = v[1];
                random.SetRandomNumber(v[2]);
                string spawn = v[0] == 0 ? "SpawnGoldenTorizoChozoOrb" : "SpawnGoldenTorizoEyeBeam";
                object?[] args = v[0] == 0 ? [loaded.Head] : [loaded.Head, (ushort)0];
                typeof(RoomEnemySystem).GetMethod(spawn, hidden)!.Invoke(loaded.Enemies, args);
                projectile = loaded.Enemies.EnemyProjectiles.Single(p => p.IsActive);
                // Match the controlled native geometry; animation execution is excluded
                // so this compares the movement callback through its first handoff.
                projectile.XRadius = projectile.YRadius = 4;
                projectile.InstructionTimer = 1;
                samples++;
            }
            if (projectile is null) throw new InvalidDataException("Missing flight initializer.");
            string step = v[0] == 0 ? "RunGoldenTorizoChozoOrbPreInstruction" : "RunGoldenTorizoEyeBeamPreInstruction";
            typeof(RoomEnemySystem).GetMethod(step, hidden)!.Invoke(loaded.Enemies, [projectile, assets.LevelData]);
            ushort[] actual = [projectile.XPosition, projectile.YPosition, projectile.XSubposition,
                projectile.YSubposition, projectile.XVelocity, projectile.YVelocity,
                projectile.InstructionPointer, projectile.InstructionTimer];
            if (!actual.SequenceEqual(v.Skip(4)))
                throw new InvalidDataException($"Golden flight differs: {line}; actual={string.Join(',', actual)}.");
            rows++;
        }
        if (samples != 16 || rows < 16) throw new InvalidDataException("Incomplete Golden flight matrix.");
        Console.WriteLine($"Golden flight: {rows} original-CPU steps across {samples} trajectories match.");
        return 0;
    }
}
