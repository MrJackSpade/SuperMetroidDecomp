using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class GoldenTorizoAudit
{
    public static int CompareNativeLifecycle(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        LoadedGoldenTorizo loaded = default;
        RoomEnemyProjectileSlot? target = null;
        int rows = 0, samples = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            ushort[] v = line.Split(',').Select(x => ushort.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            if (v.Length != 13) throw new InvalidDataException("Malformed Golden lifecycle row.");
            if (v[3] == 0)
            {
                loaded = Load(bus, room, assets, false, () => { });
                foreach (var slot in loaded.Enemies.EnemyProjectiles) slot.Clear();
                loaded.Head.XPosition = 256; loaded.Head.YPosition = 384;
                loaded.Head.Parameter1 = v[1];
                loaded.Samus.XPosition = loaded.Samus.YPosition = 4096;
                var next = (Func<ushort>)typeof(RoomEnemySystem).GetField("_nextRandom", hidden)!.GetValue(loaded.Enemies)!;
                ((Bank80SystemState)next.Target!).SetRandomNumber(v[2]);
                var kind = (RoomEnemyProjectileKind)v[0];
                string spawn = kind switch
                {
                    RoomEnemyProjectileKind.GoldenTorizoChozoOrb => "SpawnGoldenTorizoChozoOrb",
                    RoomEnemyProjectileKind.GoldenTorizoSonicBoom => "SpawnGoldenTorizoSonicBoom",
                    RoomEnemyProjectileKind.GoldenTorizoEgg => "SpawnGoldenTorizoEgg",
                    RoomEnemyProjectileKind.GoldenTorizoEyeBeam => "SpawnGoldenTorizoEyeBeam",
                    _ => throw new InvalidDataException("Unexpected lifecycle kind.")
                };
                object?[] args = kind is RoomEnemyProjectileKind.GoldenTorizoEyeBeam or
                    RoomEnemyProjectileKind.GoldenTorizoSonicBoom ? [loaded.Head, (ushort)0] : [loaded.Head];
                typeof(RoomEnemySystem).GetMethod(spawn, hidden)!.Invoke(loaded.Enemies, args);
                target = loaded.Enemies.EnemyProjectiles.Single(p => p.IsActive);
                samples++;
            }
            if (target is null) throw new InvalidDataException("Missing lifecycle initializer.");
            loaded.Enemies.StepEnemyProjectiles(assets.LevelData, loaded.Samus,
                cameraX: CameraX, cameraY: CameraY, nmiFrameCounter8: unchecked((byte)v[3]));
            int[] actual = !target.IsActive ? new int[9] : [(ushort)target.Kind,
                target.XPosition, target.YPosition, target.XVelocity, target.YVelocity,
                target.InstructionPointer, target.InstructionTimer, target.SpritemapPointer,
                target.CanDamageSamus ? 1 : 0];
            if (!actual.SequenceEqual(v.Skip(4).Select(x => (int)x)))
                throw new InvalidDataException($"Golden lifecycle differs: {line}; actual={string.Join(',', actual)}.");
            rows++;
        }
        if (samples != 16 || rows < 16) throw new InvalidDataException("Incomplete lifecycle matrix.");
        Console.WriteLine($"Golden lifecycle: {rows} original-CPU frames across {samples} samples match.");
        return 0;
    }
}
